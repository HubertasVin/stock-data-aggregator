using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockDataAggregator.Application.Dtos;
using StockDataAggregator.Application.Interfaces;

namespace StockDataAggregator.Infrastructure.Yahoo;

public class YahooClient : IYahooClient
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;
    private readonly ILogger<YahooClient> _log;

    public YahooClient(HttpClient http, IOptions<YahooClientOptions> opt, ILogger<YahooClient> log)
    {
        _http = http;
        _log = log;

        _http.BaseAddress = new Uri(opt.Value.BaseUrl.TrimEnd('/') + "/");
        _http.DefaultRequestHeaders.Remove("X-API-KEY");
        _http.DefaultRequestHeaders.Add("X-API-KEY", opt.Value.ApiKey);
        _http.DefaultRequestHeaders.Remove("accept");
        _http.DefaultRequestHeaders.Add("accept", "application/json");
    }

    public async Task<EsgScoresDto?> FetchEsgScoresAsync(string symbol)
    {
        var path = $"finance/quoteSummary/{symbol}?lang=en&region=US&modules=esgScores";

        using var req = new HttpRequestMessage(HttpMethod.Get, path);
        if (!_http.DefaultRequestHeaders.Contains("X-API-KEY"))
            req.Headers.TryAddWithoutValidation(
                "X-API-KEY",
                _http.DefaultRequestHeaders.GetValues("X-API-KEY")
            );
        req.Headers.TryAddWithoutValidation("accept", "application/json");
        req.Headers.TryAddWithoutValidation("User-Agent", "StockDataAggregator/1.0");

        using var resp = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        var text = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            _log.LogWarning(
                "Yahoo ESG request failed for {Symbol}: {Status} {Reason}. Body: {Body}",
                symbol,
                (int)resp.StatusCode,
                resp.ReasonPhrase,
                text
            );
            return null;
        }

        YahooEsgResponse? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<YahooEsgResponse>(text, JsonOpts);
        }
        catch (Exception ex)
        {
            _log.LogError(
                ex,
                "Failed to parse Yahoo ESG JSON for {Symbol}. Body: {Body}",
                symbol,
                text
            );
            return null;
        }

        var esg = parsed?.QuoteSummary?.Result?.FirstOrDefault()?.EsgScores;
        if (esg is null)
        {
            _log.LogWarning("Yahoo ESG empty for {Symbol}. Body: {Body}", symbol, text);
            return null;
        }

        DateTime? pub =
            esg.RatingYear is int y && esg.RatingMonth is int m && m is >= 1 and <= 12
                ? DateTime.SpecifyKind(new DateTime(y, m, 1), DateTimeKind.Utc)
                : null;

        return new EsgScoresDto
        {
            Total = esg.TotalEsg?.Raw ?? 0m,
            Environment = esg.EnvironmentScore?.Raw ?? 0m,
            Social = esg.SocialScore?.Raw ?? 0m,
            Governance = esg.GovernanceScore?.Raw ?? 0m,
            PublicationDate = pub ?? DateTime.UtcNow,
        };
    }

    public sealed record YahooEsgResponse
    {
        public QuoteSummaryNode? QuoteSummary { get; init; }

        public sealed record QuoteSummaryNode
        {
            public List<ResultItem>? Result { get; init; }
            public object? Error { get; init; }
        }

        public sealed record ResultItem
        {
            public EsgScores? EsgScores { get; init; }
        }

        public sealed record EsgScores
        {
            public EsgValue? TotalEsg { get; init; }
            public EsgValue? EnvironmentScore { get; init; }
            public EsgValue? SocialScore { get; init; }
            public EsgValue? GovernanceScore { get; init; }
            public int? RatingYear { get; init; }
            public int? RatingMonth { get; init; }
        }

        public sealed record EsgValue
        {
            public decimal? Raw { get; init; }
            public string? Fmt { get; init; }
        }
    }
}
