using StockDataAggregator.Application.Dtos;

namespace StockDataAggregator.Application.Interfaces;

public interface IMarketDataClient
{
    Task<SymbolMetricsDto?> FetchAsync(string symbol);
}
