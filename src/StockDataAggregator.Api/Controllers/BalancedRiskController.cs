using Microsoft.AspNetCore.Mvc;
using StockDataAggregator.Application.Dtos;
using StockDataAggregator.Application.Interfaces;

namespace StockDataAggregator.Api.Controllers;

[ApiController]
[Route("api/v1/balancedrisk")]
public class BalancedRiskMetricsController : ControllerBase
{
    private readonly ISymbolMetricsRepository _repo;

    public BalancedRiskMetricsController(ISymbolMetricsRepository repo) => _repo = repo;

    [HttpGet("{symbol}")]
    public async Task<ActionResult<BalancedRiskMetricsDto>> Get(string symbol)
    {
        var dto = await _repo.GetBalancedRiskAsync(symbol);
        return dto is not null ? Ok(dto) : NotFound();
    }
}
