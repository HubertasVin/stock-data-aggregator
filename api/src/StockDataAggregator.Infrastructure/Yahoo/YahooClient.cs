using System.Text.Json;
using Microsoft.Extensions.Logging;
using StockDataAggregator.Application.Dtos;
using StockDataAggregator.Application.Interfaces;

namespace StockDataAggregator.Infrastructure.Yahoo;

public class YahooDataClient : IMarketDataClient
{
    private static readonly JsonSerializerOptions Ser = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;
    private readonly ILogger<YahooDataClient> _log;

    public YahooDataClient(HttpClient http, ILogger<YahooDataClient> log)
    {
        _http = http;
        _log = log;
    }

    public async Task<SymbolMetricsDto?> FetchAsync(string symbol)
    {
        var modules = string.Join(
            ",",
            "earnings",
            "earningsTrend",
            "summaryDetail",
            "defaultKeyStatistics",
            "financialData",
            "cashflowStatementHistory",
            "esgScores"
        );

        var url =
            $"finance/quoteSummary/{symbol}?lang=en&region=US&modules={Uri.EscapeDataString(modules)}";

        using var resp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        var text = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
        {
            _log.LogWarning(
                "Yahoo summary failed {Symbol}: {Status} {Reason}. Body: {Body}",
                symbol,
                (int)resp.StatusCode,
                resp.ReasonPhrase,
                text
            );
            return null;
        }

        using var doc = JsonDocument.Parse(text);
        var result = doc.RootElement.GetProperty("quoteSummary").GetProperty("result")[0];

        static bool TryGet(JsonElement parent, string prop, out JsonElement value)
        {
            if (parent.ValueKind == JsonValueKind.Object && parent.TryGetProperty(prop, out var v))
            {
                value = v;
                return true;
            }
            value = default;
            return false;
        }

        static decimal? TryGetRawDecimal(JsonElement node)
        {
            if (node.ValueKind == JsonValueKind.Object && node.TryGetProperty("raw", out var raw))
                return raw.ValueKind == JsonValueKind.Number && raw.TryGetDecimal(out var d)
                    ? d
                    : null;
            if (node.ValueKind == JsonValueKind.Number && node.TryGetDecimal(out var bare))
                return bare;
            return null;
        }

        static decimal? TryGetDecimal(JsonElement root, params string[] path)
        {
            var cur = root;
            foreach (var p in path)
            {
                if (!cur.TryGetProperty(p, out var nx))
                    return null;
                cur = nx;
            }
            return TryGetRawDecimal(cur);
        }

        static string? TryGetString(JsonElement root, params string[] path)
        {
            var cur = root;
            foreach (var p in path)
            {
                if (!cur.TryGetProperty(p, out var nx))
                    return null;
                cur = nx;
            }
            if (cur.ValueKind == JsonValueKind.String)
                return cur.GetString();
            if (
                cur.ValueKind == JsonValueKind.Object
                && cur.TryGetProperty("raw", out var raw)
                && raw.ValueKind == JsonValueKind.String
            )
                return raw.GetString();
            return null;
        }

        static decimal ExtractFreeCashFlow(JsonElement resultRoot)
        {
            if (
                TryGet(resultRoot, "financialData", out var financialData)
                && TryGet(financialData, "freeCashflow", out var freeCashflowNode)
            )
            {
                var f = TryGetRawDecimal(freeCashflowNode);
                if (f is decimal d1)
                    return d1;
            }

            if (
                TryGet(resultRoot, "cashflowStatementHistory", out var cfh)
                && TryGet(cfh, "cashflowStatements", out var statements)
                && statements.ValueKind == JsonValueKind.Array
                && statements.GetArrayLength() > 0
            )
            {
                var first = statements[0];

                if (TryGet(first, "freeCashFlow", out var fcfNode))
                {
                    var f = TryGetRawDecimal(fcfNode);
                    if (f is decimal d2)
                        return d2;
                }

                decimal op = 0,
                    capex = 0;
                bool haveOp = false,
                    haveCapex = false;

                if (TryGet(first, "totalCashFromOperatingActivities", out var opNode))
                {
                    var v = TryGetRawDecimal(opNode);
                    if (v is decimal d)
                    {
                        op = d;
                        haveOp = true;
                    }
                }

                if (TryGet(first, "capitalExpenditures", out var capexNode))
                {
                    var v = TryGetRawDecimal(capexNode);
                    if (v is decimal d)
                    {
                        capex = d;
                        haveCapex = true;
                    }
                }

                if (haveOp && haveCapex)
                    return op - capex;
            }

            return 0m;
        }

        var revSeries = new List<YearValue>();
        var earnSeries = new List<YearValue>();
        if (
            result.TryGetProperty("earnings", out var earningsNode)
            && earningsNode.TryGetProperty("financialsChart", out var fc)
            && fc.TryGetProperty("yearly", out var yearly)
            && yearly.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var y in yearly.EnumerateArray())
            {
                var year = y.GetProperty("date").GetInt32();
                var rev = TryGetRawDecimal(y.GetProperty("revenue"));
                var earn = TryGetRawDecimal(y.GetProperty("earnings"));
                revSeries.Add(new YearValue { Year = year, Value = rev });
                earnSeries.Add(new YearValue { Year = year, Value = earn });
            }
        }
        revSeries.Sort((a, b) => a.Year.CompareTo(b.Year));
        earnSeries.Sort((a, b) => a.Year.CompareTo(b.Year));

        static decimal OneYearSales(List<YearValue> s)
        {
            if (s.Count < 2)
                return 0m;
            var last = s[^1].Value;
            var prev = s[^2].Value;
            return (last.HasValue && prev.HasValue && prev > 0m && last > 0m)
                ? (last.Value / prev.Value) - 1m
                : 0m;
        }

        static decimal FourYearChain(List<YearValue> arr)
        {
            if (arr.Count <= 1)
                return 0m;
            var win = arr.Skip(Math.Max(0, arr.Count - 4)).ToList();
            decimal prod = 1m;
            for (int i = 1; i < win.Count; i++)
            {
                var a = win[i - 1];
                var b = win[i];
                decimal ratio =
                    (a.Value.HasValue && b.Value.HasValue && a.Value > 0m && b.Value > 0m)
                        ? b.Value.Value / a.Value.Value
                        : 1m;
                prod *= ratio;
            }
            return prod - 1m;
        }

        var oneYearSales = OneYearSales(revSeries);
        var fourYearSales = FourYearChain(revSeries);
        var fourYearEarnings = FourYearChain(earnSeries);

        decimal? forwardPE =
            TryGetDecimal(result, "summaryDetail", "forwardPE")
            ?? TryGetDecimal(result, "defaultKeyStatistics", "forwardPE");
        decimal? trailingPE =
            TryGetDecimal(result, "summaryDetail", "trailingPE")
            ?? TryGetDecimal(result, "defaultKeyStatistics", "trailingPE");

        decimal? growthFrac = null;
        if (
            result.TryGetProperty("earningsTrend", out var et)
            && et.TryGetProperty("trend", out var tr)
            && tr.ValueKind == JsonValueKind.Array
        )
        {
            foreach (var p in new[] { "+1y", "0y", "+2y" })
            {
                foreach (var item in tr.EnumerateArray())
                {
                    if (item.TryGetProperty("period", out var per) && per.GetString() == p)
                    {
                        var g = TryGetDecimal(item, "growth");
                        if (g.HasValue)
                        {
                            growthFrac = g;
                            break;
                        }
                    }
                }
                if (growthFrac.HasValue)
                    break;
            }
        }
        if (!growthFrac.HasValue)
            growthFrac = TryGetDecimal(result, "financialData", "earningsGrowth");
        if (!growthFrac.HasValue)
        {
            var fwdEps = TryGetDecimal(result, "financialData", "forwardEps");
            var trlEps = TryGetDecimal(result, "defaultKeyStatistics", "trailingEps");
            if (fwdEps.HasValue && trlEps.HasValue && trlEps.Value != 0m)
                growthFrac = (fwdEps.Value - trlEps.Value) / Math.Abs(trlEps.Value);
        }

        decimal? chosenPE = forwardPE ?? trailingPE;
        decimal? peg =
            (chosenPE.HasValue && growthFrac.HasValue && Math.Abs(growthFrac.Value) >= 0.001m)
                ? chosenPE.Value / (growthFrac.Value * 100m)
                : null;

        var roe = TryGetDecimal(result, "financialData", "returnOnEquity") ?? 0m;
        var de = TryGetDecimal(result, "financialData", "debtToEquity") / 100 ?? 0m;
        var fcf = ExtractFreeCashFlow(result);

        decimal? dividendYield =
            TryGetDecimal(result, "summaryDetail", "dividendYield")
            ?? TryGetDecimal(result, "summaryDetail", "trailingAnnualDividendYield");

        decimal esgTotal = 0m,
            esgEnv = 0m,
            esgSoc = 0m,
            esgGov = 0m;
        DateTime? esgPub = null;
        if (
            result.TryGetProperty("esgScores", out var esg)
            && esg.ValueKind == JsonValueKind.Object
        )
        {
            esgTotal = TryGetDecimal(esg, "totalEsg") ?? 0m;
            esgEnv = TryGetDecimal(esg, "environmentScore") ?? 0m;
            esgSoc = TryGetDecimal(esg, "socialScore") ?? 0m;
            esgGov = TryGetDecimal(esg, "governanceScore") ?? 0m;

            int? y = null,
                m = null;
            if (esg.TryGetProperty("ratingYear", out var ry) && ry.TryGetInt32(out var yi))
                y = yi;
            if (esg.TryGetProperty("ratingMonth", out var rm) && rm.TryGetInt32(out var mi))
                m = mi;
            if (y.HasValue && m.HasValue && m is >= 1 and <= 12)
                esgPub = DateTime.SpecifyKind(new DateTime(y.Value, m.Value, 1), DateTimeKind.Utc);
        }

        var currency =
            TryGetString(result, "financialData", "financialCurrency")
            ?? TryGetString(result, "summaryDetail", "currency")
            ?? "";

        return new SymbolMetricsDto
        {
            Symbol = symbol,
            Date = DateTime.UtcNow.Date,
            UpdateDate = DateTime.UtcNow,

            Currency = currency,

            OneYearSalesGrowth = oneYearSales,
            FourYearSalesGrowth = fourYearSales,
            FourYearEarningsGrowth = fourYearEarnings,

            FreeCashFlow = fcf,
            DebtToEquity = de,
            PegRatio = peg ?? 0m,
            ReturnOnEquity = roe,

            DividendYield = dividendYield,

            EsgTotal = esgTotal,
            EsgEnvironment = esgEnv,
            EsgSocial = esgSoc,
            EsgGovernance = esgGov,
            EsgPublicationDate = esgPub,

            RevenueYearly = revSeries,
            EarningsYearly = earnSeries,
        };
    }
}
