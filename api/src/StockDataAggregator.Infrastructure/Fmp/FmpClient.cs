using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockDataAggregator.Application.Dtos;
using StockDataAggregator.Application.Interfaces;

namespace StockDataAggregator.Infrastructure.FmpApiClient;

public sealed class FmpClient : IFmpClient
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;
    private readonly ILogger<FmpClient> _log;
    private readonly string _apiKey;
    private readonly IYahooClient _yahoo;

    public FmpClient(
        HttpClient http,
        IOptions<FmpClientOptions> opt,
        ILogger<FmpClient> log,
        IYahooClient yahoo
    )
    {
        _http = http;
        _log = log;
        _apiKey = opt.Value.ApiKey;
        _yahoo = yahoo;
        _http.BaseAddress = new Uri(opt.Value.BaseUrl.TrimEnd('/') + "/");
    }

    public async Task<SymbolMetricsDto?> FetchAsync(string symbol)
    {
        var growthTask = GetFirstAsync<FinancialGrowth>(
            $"financial-growth?symbol={symbol}&limit=1"
        );
        var cfTask = GetFirstAsync<CashFlowStatement>(
            $"cash-flow-statement?symbol={symbol}&limit=1"
        );
        var ratioTask = GetFirstAsync<Ratio>($"ratios?symbol={symbol}&limit=1");
        var keyMetricsTask = GetFirstAsync<KeyMetrics>($"key-metrics?symbol={symbol}&limit=1");
        var esgTask = _yahoo.FetchEsgScoresAsync(symbol);
        await Task.WhenAll(growthTask, cfTask, ratioTask, keyMetricsTask, esgTask);

        var growth = await growthTask;
        var fcf = await cfTask;
        var r = await ratioTask;
        var km = await keyMetricsTask;
        var esg = await esgTask;
        if (growth is null || fcf is null || r is null || km is null || esg is null)
            return null;

        return new SymbolMetricsDto
        {
            Symbol = symbol,
            Date = DateTime.Parse(growth.date),
            UpdateDate = DateTime.UtcNow,
            OneYearSalesGrowth = growth.revenueGrowth,
            FiveYearSalesGrowth = growth.fiveYRevenueGrowthPerShare,
            FiveYearEarningsGrowth = growth.fiveYNetIncomeGrowthPerShare,
            FreeCashFlow = fcf.freeCashFlow,
            DebtToEquity = r.debtToEquityRatio,
            PegRatio = r.priceToEarningsGrowthRatio,
            ReturnOnEquity = km.returnOnEquity,
            EsgTotal = esg.Total,
            EsgEnvironment = esg.Environment,
            EsgSocial = esg.Social,
            EsgGovernance = esg.Governance,
            EsgPublicationDate = esg.PublicationDate,
        };
    }

    private async Task<T?> GetFirstAsync<T>(string relativePath)
        where T : class
    {
        try
        {
            var separator = relativePath.Contains('?') ? '&' : '?';
            var fullPath = $"{relativePath}{separator}apikey={_apiKey}";
            var json = await _http.GetStringAsync(fullPath);
            // _log.LogDebug("Response for {Path}: {Json}", _http.BaseAddress + fullPath, json);

            if (string.IsNullOrWhiteSpace(json) || json == "[]")
            {
                _log.LogWarning("Received empty response for {Path}", fullPath);
                return null;
            }

            var arr = JsonSerializer.Deserialize<T[]>(json, JsonOpts);
            return arr?.FirstOrDefault();
        }
        catch (HttpRequestException ex)
        {
            _log.LogError(ex, "HTTP request failed for {Path}", relativePath);
            return null;
        }
    }

    private sealed record FinancialGrowth(
        string date,
        decimal revenueGrowth,
        decimal fiveYRevenueGrowthPerShare,
        decimal fiveYNetIncomeGrowthPerShare
    );

    private sealed record CashFlowStatement(string date, decimal freeCashFlow);

    private sealed record Ratio(
        string date,
        decimal debtToEquityRatio,
        decimal priceToEarningsGrowthRatio
    );

    private sealed record KeyMetrics(decimal returnOnEquity);
}
