using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockDataAggregator.Persistence.Entities;

public class SymbolMetrics
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(10)]
    public string Symbol { get; set; } = null!;

    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(10)]
    public string Currency { get; set; } = "";

    public decimal OneYearSalesGrowth { get; set; }
    public decimal FourYearSalesGrowth { get; set; }
    public decimal FourYearEarningsGrowth { get; set; }

    public decimal FreeCashFlow { get; set; }
    public decimal DebtToEquity { get; set; }
    public decimal PegRatio { get; set; }
    public decimal ReturnOnEquity { get; set; }

    public decimal EsgTotal { get; set; }
    public decimal EsgEnvironment { get; set; }
    public decimal EsgSocial { get; set; }
    public decimal EsgGovernance { get; set; }

    [Column(TypeName = "date")]
    public DateTime? EsgPublicationDate { get; set; }
}
