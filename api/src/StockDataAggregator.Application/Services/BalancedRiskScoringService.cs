using Microsoft.Extensions.Options;
using StockDataAggregator.Application.Dtos;
using StockDataAggregator.Application.Interfaces;
using StockDataAggregator.Application.Options;

namespace StockDataAggregator.Application.Services;

public sealed class BalancedRiskScoringService
{
    private readonly ISymbolMetricsRepository _repo;
    private readonly IOptionsMonitor<BalancedRiskBoundsOptions> _bounds;

    public BalancedRiskScoringService(
        ISymbolMetricsRepository repo,
        IOptionsMonitor<BalancedRiskBoundsOptions> bounds
    )
    {
        _repo = repo;
        _bounds = bounds;
    }

    public async Task<BalancedRiskAnalysisDto?> AnalyzeAsync(string symbol)
    {
        var universe = await _repo.GetAllLatestPerSymbolAsync();
        if (universe.Count == 0)
            return null;

        var latest =
            universe.FirstOrDefault(x => x.Symbol == symbol) ?? await _repo.GetLatestAsync(symbol);
        if (latest is null)
            return null;

        var b = _bounds.CurrentValue;

        var valsFCF = universe.Select(v => v.FreeCashFlow).OrderBy(v => v).ToList();
        var fcfMedian =
            valsFCF.Count == 0
                ? 0m
                : (valsFCF[valsFCF.Count / 2] + valsFCF[(valsFCF.Count - 1) / 2]) / 2m;

        double s1y = ScoreHigher(
            latest.OneYearSalesGrowth,
            b.OneYearSalesGrowth,
            fallbackHiFactor: 2.0
        );
        double s5yS = ScoreHigher(
            latest.FiveYearSalesGrowth,
            b.FiveYearSalesGrowth,
            fallbackHiFactor: 1.5
        );
        double s5yE = ScoreHigher(
            latest.FiveYearEarningsGrowth,
            b.FiveYearEarningsGrowth,
            fallbackHiFactor: 1.5
        );

        double sFCF = ScoreFcf(latest.FreeCashFlow, b.FreeCashFlow, fcfMedian);

        double sDE = ScoreLower(
            CleanDebtToEquity(latest.DebtToEquity),
            b.DebtToEquity,
            idealLow: 0m
        );
        double sPEG = ScoreLower(CleanPeg(latest.PegRatio), b.PegRatio, idealLow: 0m);

        double sROE = ScoreHigher(latest.ReturnOnEquity, b.ReturnOnEquity, fallbackHiFactor: 2.0);

        const double W_1Y = 0.15;
        const double W_5YS = 0.10;
        const double W_5YE = 0.10;
        const double W_ROE = 0.20;
        const double W_FCF = 0.10;
        const double W_DE = 0.05;
        const double W_PEG = 0.30;

        double composite =
            W_PEG * sPEG
            + W_ROE * sROE
            + W_5YE * s5yE
            + W_5YS * s5yS
            + W_1Y * s1y
            + W_DE * sDE
            + W_FCF * sFCF;

        double raw = Math.Clamp(composite, 0.0, 1.0);
        int score = (int)Math.Clamp(Math.Round(1 + 9 * raw, MidpointRounding.AwayFromZero), 1, 10);

        return BuildDto(latest, score, b);
    }

    // Higher is better. If both bounds exist, linear between [lower..upper].
    // If only lower exists, we linearly ramp from lower to (lower * fallbackHiFactor).
    private static double ScoreHigher(
        decimal value,
        BalancedRiskBoundsOptions.MetricBounds mb,
        double fallbackHiFactor
    )
    {
        var lo = mb.Lower;
        var up = mb.Upper;

        if (lo is null && up is null)
            return value <= 0 ? 0.0 : 1.0;

        if (up is not null && lo is not null && up > lo)
            return Linear((double)(value - lo.Value), 0.0, (double)(up.Value - lo.Value));

        if (lo is not null)
        {
            var hi = lo.Value * (decimal)fallbackHiFactor;
            if (hi <= lo.Value)
                hi = lo.Value + (decimal)0.10;
            return Linear((double)(value - lo.Value), 0.0, (double)(hi - lo.Value));
        }

        if (up is not null)
            return Linear((double)(up.Value - value), 0.0, (double)up.Value);

        return 0.5;
    }

    // Lower is better. If both bounds exist, linear between [lower..upper] inverted.
    // If only upper exists, map [0..upper] → [1..0].
    private static double ScoreLower(
        decimal value,
        BalancedRiskBoundsOptions.MetricBounds mb,
        decimal idealLow
    )
    {
        var lo = mb.Lower ?? idealLow;
        var up = mb.Upper;

        if (up is not null && up > lo)
            return Linear((double)(up.Value - (decimal)value), 0.0, (double)(up.Value - lo));

        if (up is not null)
            return Linear((double)(up.Value - (decimal)value), 0.0, (double)up.Value);

        return Linear((double)(lo - (decimal)value), 0.0, (double)lo);
    }

    private static double ScoreFcf(
        decimal value,
        BalancedRiskBoundsOptions.MetricBounds mb,
        decimal median
    )
    {
        if (value <= 0)
            return 0.0;
        var scale = Math.Max(1m, median * 2m);
        var s = Math.Log(1.0 + (double)value) / Math.Log(1.0 + (double)scale);
        return Math.Clamp(s, 0.0, 1.0);
    }

    // piecewise linear helper: map (x in [0..range]) to [0..1], clamp.
    private static double Linear(double x, double min, double range)
    {
        if (range <= 0.0)
            return 0.5;
        var t = (x - min) / range;
        return Math.Clamp(t, 0.0, 1.0);
    }

    // Negative PEG is usually undefined (negative earnings). Treat as mediocre rather than catastrophically bad
    private static decimal CleanPeg(decimal x)
    {
        if (x <= 0)
            return 3.0m;
        if (x > 1000m)
            return 1000m;
        return x;
    }

    private static decimal CleanDebtToEquity(decimal x)
    {
        if (x < 0)
            return 0m;
        if (x > 1000m)
            return 1000m;
        return x;
    }

    private static BalancedRiskAnalysisDto BuildDto(
        SymbolMetricsDto e,
        int score,
        BalancedRiskBoundsOptions b
    ) =>
        new()
        {
            Symbol = e.Symbol,
            Date = e.Date,
            UpdateDate = e.UpdateDate,
            OneYearSalesGrowth = e.OneYearSalesGrowth,
            FiveYearSalesGrowth = e.FiveYearSalesGrowth,
            FiveYearEarningsGrowth = e.FiveYearEarningsGrowth,
            FreeCashFlow = e.FreeCashFlow,
            DebtToEquity = e.DebtToEquity,
            PegRatio = e.PegRatio,
            ReturnOnEquity = e.ReturnOnEquity,

            OneYearSalesGrowthBounds = new MetricBounds
            {
                Lower = b.OneYearSalesGrowth.Lower,
                Upper = b.OneYearSalesGrowth.Upper,
            },
            FiveYearSalesGrowthBounds = new MetricBounds
            {
                Lower = b.FiveYearSalesGrowth.Lower,
                Upper = b.FiveYearSalesGrowth.Upper,
            },
            FiveYearEarningsGrowthBounds = new MetricBounds
            {
                Lower = b.FiveYearEarningsGrowth.Lower,
                Upper = b.FiveYearEarningsGrowth.Upper,
            },
            FreeCashFlowBounds = new MetricBounds
            {
                Lower = b.FreeCashFlow.Lower,
                Upper = b.FreeCashFlow.Upper,
            },
            DebtToEquityBounds = new MetricBounds
            {
                Lower = b.DebtToEquity.Lower,
                Upper = b.DebtToEquity.Upper,
            },
            PegRatioBounds = new MetricBounds
            {
                Lower = b.PegRatio.Lower,
                Upper = b.PegRatio.Upper,
            },
            ReturnOnEquityBounds = new MetricBounds
            {
                Lower = b.ReturnOnEquity.Lower,
                Upper = b.ReturnOnEquity.Upper,
            },

            Score = score,
        };
}
