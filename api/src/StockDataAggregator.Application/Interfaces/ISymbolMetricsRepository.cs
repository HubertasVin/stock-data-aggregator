using StockDataAggregator.Application.Dtos;

namespace StockDataAggregator.Application.Interfaces;

public interface ISymbolMetricsRepository
{
    Task UpsertAsync(SymbolMetricsDto dto);
    Task<SymbolMetricsDto?> GetLatestAsync(string symbol);
    Task<IEnumerable<string>> GetAllSymbolsAsync();
    Task AddSymbolAsync(string symbol);
    Task<DateTime?> GetUpdateDateAsync(string symbol);
    Task<BalancedRiskMetricsDto?> GetBalancedRiskAsync(string symbol);
}
