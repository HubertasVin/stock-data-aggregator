namespace StockDataAggregator.Application.Options;

public sealed class BalancedRiskBoundsOptions
{
    public MetricBounds OneYearSalesGrowth { get; set; } = new() { Lower = 0.05m, Upper = null };
    public MetricBounds FiveYearSalesGrowth { get; set; } = new() { Lower = 0.50m, Upper = null };
    public MetricBounds FiveYearEarningsGrowth { get; set; } =
        new() { Lower = 0.10m, Upper = null };
    public MetricBounds ReturnOnEquity { get; set; } = new() { Lower = 0.15m, Upper = null };
    public MetricBounds DebtToEquity { get; set; } = new() { Lower = 0m, Upper = 1m };
    public MetricBounds FreeCashFlow { get; set; } = new() { Lower = 0m, Upper = null };
    public MetricBounds PegRatio { get; set; } = new() { Lower = 0m, Upper = 2m };

    public sealed class MetricBounds
    {
        public decimal? Lower { get; set; }
        public decimal? Upper { get; set; }
    }
}
