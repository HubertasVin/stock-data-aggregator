using StockDataAggregator.Application.Dtos;
using StockDataAggregator.Application.Interfaces;

namespace StockDataAggregator.Application.Services;

public class BalancedRiskDataService
{
    private readonly IFmpClient _fetcher;
    private readonly ISymbolMetricsRepository _repo;

    public BalancedRiskDataService(IFmpClient fetcher, ISymbolMetricsRepository repo)
    {
        _fetcher = fetcher;
        _repo = repo;
    }

    public async Task RefreshAsync(string symbol)
    {
        var dto = await _fetcher.FetchAsync(symbol);
        if (dto is not null)
            await _repo.UpsertAsync(dto);
    }
}
