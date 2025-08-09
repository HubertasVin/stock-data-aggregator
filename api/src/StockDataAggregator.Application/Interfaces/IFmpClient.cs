using StockDataAggregator.Application.Dtos;

namespace StockDataAggregator.Application.Interfaces;

public interface IFmpClient
{
    Task<SymbolMetricsDto?> FetchAsync(string symbol);
}
