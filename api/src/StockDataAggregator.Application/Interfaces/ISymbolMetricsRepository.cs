using StockDataAggregator.Application.Dtos;

namespace StockDataAggregator.Application.Interfaces;

public interface ISymbolMetricsRepository
{
    Task UpsertAsync(SymbolMetricsDto dto);
    Task<SymbolMetricsDto?> GetLatestAsync(string symbol);
    Task<IReadOnlyList<SymbolMetricsDto>> GetAllLatestPerSymbolAsync();
    Task<IEnumerable<string>> GetAllSymbolsAsync();
    Task AddSymbolAsync(string symbol);
    Task<DateTime?> GetUpdateDateAsync(string symbol);
    Task<BalancedRiskAnalysisDto?> GetBalancedRiskAsync(string symbol);
}
