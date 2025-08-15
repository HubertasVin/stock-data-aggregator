using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockDataAggregator.Persistence.Entities;

public class FinancialsYearly
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(10)]
    public string Symbol { get; set; } = null!;

    public int Year { get; set; }

    [Column(TypeName = "numeric")]
    public decimal Revenue { get; set; }

    [Column(TypeName = "numeric")]
    public decimal Earnings { get; set; }
}
