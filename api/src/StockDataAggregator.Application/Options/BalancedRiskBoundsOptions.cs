using StockDataAggregator.Application.Dtos;

namespace StockDataAggregator.Application.Options;

public sealed class BalancedRiskBoundsOptions
{
    public MetricBounds OneYearSalesGrowth { get; set; } = new() { Lower = 0.05m, Upper = null };
    public MetricBounds FourYearSalesGrowth { get; set; } = new() { Lower = 0.50m, Upper = null };
    public MetricBounds FourYearEarningsGrowth { get; set; } =
        new() { Lower = 0.10m, Upper = null };
    public MetricBounds ReturnOnEquity { get; set; } = new() { Lower = 0.15m, Upper = null };
    public MetricBounds DebtToEquity { get; set; } = new() { Lower = 0m, Upper = 1m };
    public MetricBounds FreeCashFlow { get; set; } = new() { Lower = 0m, Upper = null };
    public MetricBounds PegRatio { get; set; } = new() { Lower = 0m, Upper = 2m };
}
