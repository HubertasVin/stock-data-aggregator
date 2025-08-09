using StockDataAggregator.Application.Dtos;

namespace StockDataAggregator.Application.Interfaces;

public interface IYahooClient
{
    Task<EsgScoresDto?> FetchEsgScoresAsync(string symbol);
}
