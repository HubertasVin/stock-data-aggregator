using StockDataAggregator.Application.Interfaces;

namespace StockDataAggregator.Api.HostedServices;

public class PeriodicFetchHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PeriodicFetchHostedService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromDays(1);

    public PeriodicFetchHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<PeriodicFetchHostedService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "PeriodicFetchHostedService starting, interval {Interval}.",
            _interval
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<ISymbolMetricsRepository>();
                var fetcher = scope.ServiceProvider.GetRequiredService<IMarketDataClient>();

                var now = DateTime.UtcNow;
                var symbols = await repo.GetAllSymbolsAsync();
                foreach (var symbol in symbols)
                {
                    var lastUpdate = await repo.GetUpdateDateAsync(symbol) ?? DateTime.MinValue;
                    if ((now - lastUpdate).TotalDays < 7)
                    {
                        _logger.LogInformation(
                            "Skipping {Symbol}, last update was {Days:N1} days ago.",
                            symbol,
                            (now - lastUpdate).TotalDays
                        );
                        continue;
                    }

                    _logger.LogInformation("Fetching data for {Symbol}", symbol);
                    var dto = await fetcher.FetchAsync(symbol);
                    if (dto != null)
                    {
                        await repo.UpsertAsync(dto);
                        _logger.LogInformation("Upserted {Symbol} @ {Date}", dto.Symbol, dto.Date);
                    }
                    else
                    {
                        _logger.LogWarning("No data returned for {Symbol}", symbol);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during periodic fetch");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("PeriodicFetchHostedService stopping.");
    }
}
