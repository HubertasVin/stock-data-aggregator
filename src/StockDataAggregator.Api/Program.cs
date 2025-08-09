using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StockDataAggregator.Api.Options;
using StockDataAggregator.Application.Interfaces;
using StockDataAggregator.Infrastructure.FmpApiClient;
using StockDataAggregator.Infrastructure.Yahoo;
using StockDataAggregator.Persistence;
using StockDataAggregator.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// configuration
builder.Services.Configure<FmpClientOptions>(builder.Configuration.GetSection("FmpClient"));
builder.Services.Configure<YahooClientOptions>(builder.Configuration.GetSection("YahooClient"));
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

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// DI
builder.Services.AddScoped<ISymbolMetricsRepository, EfSymbolMetricsRepository>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<IFmpClient, FmpClient>();
builder.Services.AddHttpClient<IYahooClient, YahooClient>(
    (sp, client) =>
    {
        var opt = sp.GetRequiredService<IOptions<YahooClientOptions>>().Value;
        client.BaseAddress = new Uri(opt.BaseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Add("X-API-KEY", opt.ApiKey);
    }
);
builder.Services.AddHostedService<StockDataAggregator.Api.HostedServices.PeriodicFetchHostedService>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapPost(
    "/api/v1/refresh/{symbol}",
    async (string symbol, IFmpClient fetcher, ISymbolMetricsRepository repo) =>
    {
        var fetched = await fetcher.FetchAsync(symbol);
        if (fetched is null)
            return Results.NotFound();

        await repo.UpsertAsync(fetched);

        var saved = await repo.GetLatestAsync(symbol);
        return saved is not null ? Results.Ok(saved) : Results.NotFound();
    }
);
app.UseAuthentication();
app.UseAuthorization();
app.Run();
