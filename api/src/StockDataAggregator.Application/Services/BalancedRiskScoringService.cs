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

        if (universe.Count < 5)
            return BuildDto(latest, 5, b);

        var vals1y = universe.Select(v => v.OneYearSalesGrowth).ToList();
        var vals5yS = universe.Select(v => v.FiveYearSalesGrowth).ToList();
        var vals5yE = universe.Select(v => v.FiveYearEarningsGrowth).ToList();
        var valsFCF = universe.Select(v => v.FreeCashFlow).ToList();
        var valsDE = universe.Select(v => CleanDebtToEquity(v.DebtToEquity)).ToList();
        var valsPEG = universe.Select(v => CleanPeg(v.PegRatio)).ToList();
        var valsROE = universe.Select(v => v.ReturnOnEquity).ToList();

        double S(
            decimal value,
            List<decimal> vec,
            bool higherIsBetter,
            BalancedRiskBoundsOptions.MetricBounds mb
        )
        {
            var p = Percentile(vec, value, higherIsBetter);
            var inBounds = InBounds(value, mb);
            var s = p * (inBounds ? 1.0 : 0.6);
            if (double.IsNaN(s) || double.IsInfinity(s))
                s = 0.5;
            return Math.Clamp(s, 0.02, 0.98);
        }

        double s1y = S(latest.OneYearSalesGrowth, vals1y, true, b.OneYearSalesGrowth);
        double s5yS = S(latest.FiveYearSalesGrowth, vals5yS, true, b.FiveYearSalesGrowth);
        double s5yE = S(latest.FiveYearEarningsGrowth, vals5yE, true, b.FiveYearEarningsGrowth);
        double sFCF = S(latest.FreeCashFlow, valsFCF, true, b.FreeCashFlow);
        double sDE = S(CleanDebtToEquity(latest.DebtToEquity), valsDE, false, b.DebtToEquity);
        double sPEG = S(CleanPeg(latest.PegRatio), valsPEG, false, b.PegRatio);
        double sROE = S(latest.ReturnOnEquity, valsROE, true, b.ReturnOnEquity);

        const double W_1Y = 0.15;
        const double W_5YS = 0.10;
        const double W_5YE = 0.10;
        const double W_ROE = 0.20;
        const double W_FCF = 0.10;
        const double W_DE = 0.05;
        const double W_PEG = 0.30;

        double CompositeFor(
            decimal oneY,
            decimal fiveYS,
            decimal fiveYE,
            decimal fcf,
            decimal de,
            decimal peg,
            decimal roe
        )
        {
            var c1y = S(oneY, vals1y, true, b.OneYearSalesGrowth);
            var c5s = S(fiveYS, vals5yS, true, b.FiveYearSalesGrowth);
            var c5e = S(fiveYE, vals5yE, true, b.FiveYearEarningsGrowth);
            var cfcf = S(fcf, valsFCF, true, b.FreeCashFlow);
            var cde = S(CleanDebtToEquity(de), valsDE, false, b.DebtToEquity);
            var cpeg = S(CleanPeg(peg), valsPEG, false, b.PegRatio);
            var croe = S(roe, valsROE, true, b.ReturnOnEquity);

            return W_PEG * cpeg
                + W_ROE * croe
                + W_5YE * c5e
                + W_5YS * c5s
                + W_1Y * c1y
                + W_DE * cde
                + W_FCF * cfcf;
        }

        var composites = universe
            .Select(u =>
                CompositeFor(
                    u.OneYearSalesGrowth,
                    u.FiveYearSalesGrowth,
                    u.FiveYearEarningsGrowth,
                    u.FreeCashFlow,
                    u.DebtToEquity,
                    u.PegRatio,
                    u.ReturnOnEquity
                )
            )
            .ToList();

        var myComposite = CompositeFor(
            latest.OneYearSalesGrowth,
            latest.FiveYearSalesGrowth,
            latest.FiveYearEarningsGrowth,
            latest.FreeCashFlow,
            latest.DebtToEquity,
            latest.PegRatio,
            latest.ReturnOnEquity
        );

        var pComp = Percentile(
            composites.Select(x => (decimal)x).ToList(),
            (decimal)myComposite,
            true
        );
        var score = Math.Clamp((int)Math.Ceiling(pComp * 10.0), 1, 10);

        return BuildDto(latest, score, b);
    }

    private static BalancedRiskAnalysisDto BuildDto(
        SymbolMetricsDto e,
        int score,
        BalancedRiskBoundsOptions b
    )
    {
        return new BalancedRiskAnalysisDto
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

    private static bool InBounds(decimal v, BalancedRiskBoundsOptions.MetricBounds b)
    {
        if (b.Lower is decimal lo && !(v > lo))
            return false;
        if (b.Upper is decimal up && !(v < up))
            return false;
        return true;
    }

    private static decimal CleanPeg(decimal x)
    {
        if (x <= 0)
            return 10m;
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

    private static double Percentile(List<decimal> values, decimal target, bool higherIsBetter)
    {
        if (values.Count == 0)
            return 0.0;
        int less = 0,
            equal = 0;
        foreach (var v in values)
        {
            if (v < target)
                less++;
            else if (v == target)
                equal++;
        }
        double p = (less + 0.5 * equal) / values.Count;
        if (!higherIsBetter)
            p = 1.0 - p;
        if (double.IsNaN(p) || double.IsInfinity(p))
            return 0.0;
        return Math.Clamp(p, 0.0, 1.0);
    }
}
