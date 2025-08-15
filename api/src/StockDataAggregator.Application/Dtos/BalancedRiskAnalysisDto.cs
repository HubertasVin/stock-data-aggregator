namespace StockDataAggregator.Application.Dtos;

public class BalancedRiskAnalysisDto
{
    public string Symbol { get; set; } = null!;
    public DateTime Date { get; set; }
    public DateTime UpdateDate { get; set; }
    public decimal OneYearSalesGrowth { get; set; }
    public decimal FourYearSalesGrowth { get; set; }
    public decimal FourYearEarningsGrowth { get; set; }
    public decimal FreeCashFlow { get; set; }
    public decimal DebtToEquity { get; set; }
    public decimal PegRatio { get; set; }
    public decimal ReturnOnEquity { get; set; }

    public MetricBounds OneYearSalesGrowthBounds { get; set; } = new();
    public MetricBounds FourYearSalesGrowthBounds { get; set; } = new();
    public MetricBounds FourYearEarningsGrowthBounds { get; set; } = new();
    public MetricBounds? FreeCashFlowBounds { get; set; } = new();
    public MetricBounds DebtToEquityBounds { get; set; } = new();
    public MetricBounds PegRatioBounds { get; set; } = new();
    public MetricBounds ReturnOnEquityBounds { get; set; } = new();

    public int Score { get; set; }
}
