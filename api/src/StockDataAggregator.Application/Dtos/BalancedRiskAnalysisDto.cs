namespace StockDataAggregator.Application.Dtos;

public class BalancedRiskAnalysisDto
{
    public string Symbol { get; set; } = null!;
    public DateTime Date { get; set; }
    public DateTime UpdateDate { get; set; }
    public decimal OneYearSalesGrowth { get; set; }
    public decimal FiveYearSalesGrowth { get; set; }
    public decimal FiveYearEarningsGrowth { get; set; }
    public decimal FreeCashFlow { get; set; }
    public decimal DebtToEquity { get; set; }
    public decimal PegRatio { get; set; }
    public decimal ReturnOnEquity { get; set; }

    public MetricBounds OneYearSalesGrowthBounds { get; set; } =
        new MetricBounds { Lower = null, Upper = null };
    public MetricBounds FiveYearSalesGrowthBounds { get; set; } =
        new MetricBounds { Lower = null, Upper = null };
    public MetricBounds FiveYearEarningsGrowthBounds { get; set; } =
        new MetricBounds { Lower = null, Upper = null };
    public MetricBounds? FreeCashFlowBounds { get; set; } =
        new MetricBounds { Lower = null, Upper = null };
    public MetricBounds DebtToEquityBounds { get; set; } =
        new MetricBounds { Lower = null, Upper = null };
    public MetricBounds PegRatioBounds { get; set; } =
        new MetricBounds { Lower = null, Upper = null };
    public MetricBounds ReturnOnEquityBounds { get; set; } =
        new MetricBounds { Lower = null, Upper = null };

    public int Score { get; set; }
}
