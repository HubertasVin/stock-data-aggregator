namespace StockDataAggregator.Application.Dtos;

public class YearValue
{
    public int Year { get; set; }
    public decimal? Value { get; set; }
}

public class SymbolMetricsDto
{
    public string Symbol { get; set; } = null!;
    public DateTime Date { get; set; }
    public DateTime UpdateDate { get; set; }

    public string Currency { get; set; } = "";

    public decimal OneYearSalesGrowth { get; set; }
    public decimal FourYearSalesGrowth { get; set; }
    public decimal FourYearEarningsGrowth { get; set; }

    public decimal FreeCashFlow { get; set; }
    public decimal DebtToEquity { get; set; }
    public decimal PegRatio { get; set; }
    public decimal ReturnOnEquity { get; set; }

    public decimal? DividendYield { get; set; }

    public decimal EsgTotal { get; set; }
    public decimal EsgEnvironment { get; set; }
    public decimal EsgSocial { get; set; }
    public decimal EsgGovernance { get; set; }
    public DateTime? EsgPublicationDate { get; set; }

    public List<YearValue> RevenueYearly { get; set; } = new();
    public List<YearValue> EarningsYearly { get; set; } = new();
}
