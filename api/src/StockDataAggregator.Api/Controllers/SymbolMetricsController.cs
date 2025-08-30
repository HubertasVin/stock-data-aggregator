using Microsoft.AspNetCore.Mvc;
using StockDataAggregator.Application.Dtos;
using StockDataAggregator.Application.Interfaces;

namespace StockDataAggregator.Api.Controllers;

[ApiController]
[Route("api/v1/metrics")]
public sealed class SymbolMetricsController : ControllerBase
{
    private readonly ISymbolMetricsRepository _repo;

    public SymbolMetricsController(ISymbolMetricsRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SymbolMetricsDto>>> GetAll() =>
        Ok(await _repo.GetAllLatestPerSymbolAsync());

    [HttpGet("{symbol}")]
    public async Task<ActionResult<SymbolMetricsDto>> Get(string symbol)
    {
        var dto = await _repo.GetLatestAsync(symbol);
        return dto is not null ? Ok(dto) : NotFound();
    }
}
