using System.Threading.Tasks;
using StockDataAggregator.Application.Interfaces;

namespace StockDataAggregator.Application.Services;

public sealed class TrackedSymbolsService
{
    private readonly IMarketDataClient _marketData;

    public TrackedSymbolsService(IMarketDataClient marketData)
    {
        _marketData = marketData;
    }

    public async Task<bool> CheckIfYahooHasSymbolAsync(string symbol)
    {
        var data = await _marketData.FetchAsync(symbol);
        if (data is null)
            return false;

        return true;
    }
}
