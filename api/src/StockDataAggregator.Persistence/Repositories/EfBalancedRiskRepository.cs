using Microsoft.EntityFrameworkCore;
using StockDataAggregator.Application.Dtos;
using StockDataAggregator.Application.Interfaces;
using StockDataAggregator.Persistence.Entities;
using StockDataAggregator.Persistence.Mappers;

namespace StockDataAggregator.Persistence.Repositories;

public class EfSymbolMetricsRepository : ISymbolMetricsRepository
{
    private readonly FinanceContext _db;

    public EfSymbolMetricsRepository(FinanceContext db) => _db = db;

    public async Task UpsertAsync(SymbolMetricsDto dto)
    {
        var now = DateTime.UtcNow;
        var existing = await _db.SymbolMetrics.FirstOrDefaultAsync(x =>
            x.Symbol == dto.Symbol && x.Date == dto.Date
        );

        if (existing is null)
        {
            _db.SymbolMetrics.Add(
                new SymbolMetrics
                {
                    Symbol = dto.Symbol,
                    Date = dto.Date,
                    UpdateDate = now,

                    Currency = dto.Currency,

                    OneYearSalesGrowth = dto.OneYearSalesGrowth,
                    FourYearSalesGrowth = dto.FourYearSalesGrowth,
                    FourYearEarningsGrowth = dto.FourYearEarningsGrowth,

                    FreeCashFlow = dto.FreeCashFlow,
                    DebtToEquity = dto.DebtToEquity,
                    PegRatio = dto.PegRatio,
                    ReturnOnEquity = dto.ReturnOnEquity,

                    EsgTotal = dto.EsgTotal,
                    EsgEnvironment = dto.EsgEnvironment,
                    EsgSocial = dto.EsgSocial,
                    EsgGovernance = dto.EsgGovernance,
                    EsgPublicationDate = dto.EsgPublicationDate,
                }
            );
        }
        else
        {
            existing.UpdateDate = now;
            existing.Currency = dto.Currency;
            existing.OneYearSalesGrowth = dto.OneYearSalesGrowth;
            existing.FourYearSalesGrowth = dto.FourYearSalesGrowth;
            existing.FourYearEarningsGrowth = dto.FourYearEarningsGrowth;

            existing.FreeCashFlow = dto.FreeCashFlow;
            existing.DebtToEquity = dto.DebtToEquity;
            existing.PegRatio = dto.PegRatio;
            existing.ReturnOnEquity = dto.ReturnOnEquity;

            existing.EsgTotal = dto.EsgTotal;
            existing.EsgEnvironment = dto.EsgEnvironment;
            existing.EsgSocial = dto.EsgSocial;
            existing.EsgGovernance = dto.EsgGovernance;
            existing.EsgPublicationDate = dto.EsgPublicationDate;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<SymbolMetricsDto?> GetLatestAsync(string symbol)
    {
        var e = await _db
            .SymbolMetrics.Where(x => x.Symbol == symbol)
            .OrderByDescending(x => x.Date)
            .FirstOrDefaultAsync();

        return e == null
            ? null
            : new SymbolMetricsDto
            {
                Symbol = e.Symbol,
                Date = e.Date,
                UpdateDate = e.UpdateDate,
                Currency = e.Currency,
                OneYearSalesGrowth = e.OneYearSalesGrowth,
                FourYearSalesGrowth = e.FourYearSalesGrowth,
                FourYearEarningsGrowth = e.FourYearEarningsGrowth,
                FreeCashFlow = e.FreeCashFlow,
                DebtToEquity = e.DebtToEquity,
                PegRatio = e.PegRatio,
                ReturnOnEquity = e.ReturnOnEquity,
                EsgTotal = e.EsgTotal,
                EsgEnvironment = e.EsgEnvironment,
                EsgSocial = e.EsgSocial,
                EsgGovernance = e.EsgGovernance,
                EsgPublicationDate = e.EsgPublicationDate,
            };
    }

    public async Task<IReadOnlyList<SymbolMetricsDto>> GetAllLatestPerSymbolAsync()
    {
        var latestDates = _db
            .SymbolMetrics.AsNoTracking()
            .GroupBy(x => x.Symbol)
            .Select(g => new { Symbol = g.Key, Date = g.Max(x => x.Date) });

        var query =
            from e in _db.SymbolMetrics.AsNoTracking()
            join ld in latestDates on new { e.Symbol, e.Date } equals new { ld.Symbol, ld.Date }
            select new SymbolMetricsDto
            {
                Symbol = e.Symbol,
                Date = e.Date,
                UpdateDate = e.UpdateDate,
                Currency = e.Currency,
                OneYearSalesGrowth = e.OneYearSalesGrowth,
                FourYearSalesGrowth = e.FourYearSalesGrowth,
                FourYearEarningsGrowth = e.FourYearEarningsGrowth,
                FreeCashFlow = e.FreeCashFlow,
                DebtToEquity = e.DebtToEquity,
                PegRatio = e.PegRatio,
                ReturnOnEquity = e.ReturnOnEquity,
                EsgTotal = e.EsgTotal,
                EsgEnvironment = e.EsgEnvironment,
                EsgSocial = e.EsgSocial,
                EsgGovernance = e.EsgGovernance,
                EsgPublicationDate = e.EsgPublicationDate,
            };

        return await query.ToListAsync();
    }

    public async Task<BalancedRiskAnalysisDto?> GetBalancedRiskAsync(string symbol)
    {
        var e = await _db
            .SymbolMetrics.Where(x => x.Symbol == symbol)
            .OrderByDescending(x => x.Date)
            .FirstOrDefaultAsync();

        return e?.ToBalancedRiskDto();
    }

    public async Task<IEnumerable<string>> GetAllSymbolsAsync() =>
        await _db.TrackedSymbols.Select(x => x.Symbol).ToListAsync();

    public async Task AddSymbolAsync(string symbol)
    {
        if (await _db.TrackedSymbols.AnyAsync(x => x.Symbol == symbol))
            return;

        _db.TrackedSymbols.Add(new TrackedSymbol { Symbol = symbol });
        await _db.SaveChangesAsync();
    }

    public async Task<DateTime?> GetUpdateDateAsync(string symbol) =>
        await _db
            .SymbolMetrics.Where(x => x.Symbol == symbol)
            .OrderByDescending(x => x.Date)
            .Select(x => (DateTime?)x.UpdateDate)
            .FirstOrDefaultAsync();
}
