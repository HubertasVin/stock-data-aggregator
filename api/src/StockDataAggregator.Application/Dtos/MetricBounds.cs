namespace StockDataAggregator.Application.Dtos;

public sealed record MetricBounds
{
    public decimal? Lower { get; set; }
    public decimal? Upper { get; set; }
}
