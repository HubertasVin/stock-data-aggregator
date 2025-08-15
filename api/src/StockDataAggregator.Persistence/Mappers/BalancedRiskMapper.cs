using System.Text.Json;
using StockDataAggregator.Application.Dtos;
using StockDataAggregator.Persistence.Entities;

namespace StockDataAggregator.Persistence.Mappers;

public static class BalancedRiskMapper
{
    private static readonly JsonSerializerOptions Ser = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static BalancedRiskAnalysisDto ToBalancedRiskDto(this SymbolMetrics e)
    {
        return new()
        {
            Symbol = e.Symbol,
            Date = e.Date,
            UpdateDate = e.UpdateDate,
            OneYearSalesGrowth = e.OneYearSalesGrowth,
            FourYearSalesGrowth = e.FourYearSalesGrowth,
            FourYearEarningsGrowth = e.FourYearEarningsGrowth,
            FreeCashFlow = e.FreeCashFlow,
            DebtToEquity = e.DebtToEquity,
            PegRatio = e.PegRatio,
            ReturnOnEquity = e.ReturnOnEquity,
        };
    }
}
