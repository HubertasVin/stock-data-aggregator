using StockDataAggregator.Application.Dtos;
using StockDataAggregator.Persistence.Entities;

namespace StockDataAggregator.Persistence.Mappers;

public static class BalancedRiskMapper
{
    public static BalancedRiskMetricsDto ToBalancedRiskDto(this SymbolMetrics e) =>
        new()
        {
            Symbol = e.Symbol,
            Date = e.Date,
            UpdateDate = e.UpdateDate,
            OneYearSalesGrowth = e.OneYearSalesGrowth,
            FiveYearSalesGrowth = e.FiveYearSalesGrowth,
            FiveYearEarningsGrowth = e.FiveYearEarningsGrowth,
            FreeCashFlow = e.FreeCashFlow,
            DebtToEquity = e.DebtToEquity,
            PegRatio = e.PegRatio,
            ReturnOnEquity = e.ReturnOnEquity,
        };
}
