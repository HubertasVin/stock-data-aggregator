using Microsoft.AspNetCore.Mvc;
using StockDataAggregator.Application.Dtos;
using StockDataAggregator.Application.Services;

namespace StockDataAggregator.Api.Controllers;

[ApiController]
[Route("api/v1/balancedrisk")]
public class BalancedRiskMetricsController : ControllerBase
{
    private readonly BalancedRiskScoringService _svc;

    public BalancedRiskMetricsController(BalancedRiskScoringService svc) => _svc = svc;

    [HttpGet("{symbol}")]
    public async Task<ActionResult<BalancedRiskAnalysisDto>> Get(string symbol)
    {
        var dto = await _svc.AnalyzeAsync(symbol);
        return dto is not null ? Ok(dto) : NotFound();
    }
}
