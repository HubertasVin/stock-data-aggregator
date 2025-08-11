using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StockDataAggregator.Api.Options;
using StockDataAggregator.Application.Interfaces;
using StockDataAggregator.Application.Options;
using StockDataAggregator.Application.Services;
using StockDataAggregator.Infrastructure.FmpApiClient;
using StockDataAggregator.Infrastructure.Yahoo;
using StockDataAggregator.Persistence;
using StockDataAggregator.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// CORS for dev; in prod we are same-origin behind nginx
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

// configuration
builder.Services.Configure<FmpClientOptions>(builder.Configuration.GetSection("FmpClient"));
builder.Services.Configure<YahooClientOptions>(builder.Configuration.GetSection("YahooClient"));
builder.Services.Configure<BalancedRiskBoundsOptions>(
    builder.Configuration.GetSection("BalancedRiskBounds")
);

builder.Services.AddDbContext<FinanceContext>(opts =>
{
    var cs = GetConnectionString(builder.Configuration);
    opts.UseNpgsql(cs);
});
static string GetConnectionString(IConfiguration cfg)
{
    var host = cfg["POSTGRES_HOST"] ?? "db";
    var db = cfg["POSTGRES_DB"] ?? "stockdata";
    var user = cfg["POSTGRES_USER"] ?? "stockuser";
    var pass = cfg["POSTGRES_PASSWORD"];

    if (string.IsNullOrWhiteSpace(pass))
        throw new InvalidOperationException(
            "Missing ConnectionStrings:Default and POSTGRES_PASSWORD."
        );

    return $"Host={host};Database={db};Username={user};Password={pass}";
}

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
builder.Services.AddScoped<BalancedRiskScoringService>();
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

// Auto-migrate DB on startup when enabled (PROD-safe if schema matches)
if (
    Environment
        .GetEnvironmentVariable("MIGRATE_ON_STARTUP")
        ?.Equals("true", StringComparison.OrdinalIgnoreCase) == true
)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FinanceContext>();
    db.Database.Migrate();
}

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

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.Run();
