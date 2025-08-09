using System.ComponentModel.DataAnnotations;

namespace StockDataAggregator.Persistence.Entities;

public class TrackedSymbol
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(10)]
    public string Symbol { get; set; } = null!;
}
