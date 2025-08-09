namespace StockDataAggregator.Application.Dtos;

public class EsgScoresDto
{
    public decimal Total { get; set; }
    public decimal Environment { get; set; }
    public decimal Social { get; set; }
    public decimal Governance { get; set; }
    public DateTime PublicationDate { get; set; }
}
