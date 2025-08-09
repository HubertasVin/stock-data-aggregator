namespace StockDataAggregator.Application.Dtos;

public class SymbolMetricsDto
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

    // ESG
    public decimal EsgTotal { get; set; }
    public decimal EsgEnvironment { get; set; }
    public decimal EsgSocial { get; set; }
    public decimal EsgGovernance { get; set; }
    public DateTime? EsgPublicationDate { get; set; }
}
