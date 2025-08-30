using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StockDataAggregator.Api.Options;
using StockDataAggregator.Application.Interfaces;
using StockDataAggregator.Application.Options;
using StockDataAggregator.Application.Services;
using StockDataAggregator.Infrastructure.Yahoo;
using StockDataAggregator.Persistence;
using StockDataAggregator.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o =>
{
    o.AddPolicy(
        "Frontend",
        p =>
            p.WithOrigins("http://localhost:5173", "http://localhost:5174")
                .AllowAnyHeader()
                .AllowAnyMethod()
    );
});

builder.Services.AddControllers();

builder.Services.Configure<YahooClientOptions>(builder.Configuration.GetSection("YahooClient"));
builder.Services.Configure<BalancedRiskBoundsOptions>(
    builder.Configuration.GetSection("BalancedRiskBounds")
);

builder.Services.AddDbContext<FinanceContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization(o => o.AddPolicy("AdminOnly", p => p.RequireRole("Admin")));

builder.Services.AddScoped<ISymbolMetricsRepository, EfSymbolMetricsRepository>();
builder.Services.AddScoped<BalancedRiskScoringService>();
builder.Services.AddScoped<TrackedSymbolsService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IMarketDataClient, YahooDataClient>(
    (sp, client) =>
    {
        var opt = sp.GetRequiredService<IOptions<YahooClientOptions>>().Value;
        client.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Remove("X-API-KEY");
        client.DefaultRequestHeaders.Add("X-API-KEY", opt.ApiKey);
        client.DefaultRequestHeaders.Remove("accept");
        client.DefaultRequestHeaders.Add("accept", "application/json");
        client.DefaultRequestHeaders.Remove("User-Agent");
        client.DefaultRequestHeaders.Add("User-Agent", "StockDataAggregator/1.0");
    }
);

builder.Services.AddHostedService<StockDataAggregator.Api.HostedServices.PeriodicFetchHostedService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FinanceContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapPost(
    "/api/v1/refresh/{symbol}",
    async (string symbol, IMarketDataClient fetcher, ISymbolMetricsRepository repo) =>
    {
        var fetched = await fetcher.FetchAsync(symbol);
        if (fetched is null)
            return Results.NotFound();

        await repo.UpsertAsync(fetched);
        var saved = await repo.GetLatestAsync(symbol);
        return saved is not null ? Results.Ok(saved) : Results.NotFound();
    }
);

app.Run();
