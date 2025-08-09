using Microsoft.EntityFrameworkCore;
using StockDataAggregator.Persistence.Entities;

namespace StockDataAggregator.Persistence;

public class FinanceContext : DbContext
{
    public FinanceContext(DbContextOptions<FinanceContext> opts)
        : base(opts) { }

    public DbSet<SymbolMetrics> SymbolMetrics { get; set; } = null!;
    public DbSet<TrackedSymbol> TrackedSymbols { get; set; } = null!;
}
