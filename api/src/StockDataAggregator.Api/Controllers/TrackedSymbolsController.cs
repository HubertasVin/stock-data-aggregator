using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockDataAggregator.Application.Interfaces;

namespace StockDataAggregator.Api.Controllers;

[ApiController]
[Route("api/v1/symbols")]
public sealed class TrackedSymbolsController : ControllerBase
{
    private readonly ISymbolMetricsRepository _repo;

    public TrackedSymbolsController(ISymbolMetricsRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> Get() =>
        Ok(await _repo.GetAllSymbolsAsync());

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Post([FromBody] string symbol)
    {
        await _repo.AddSymbolAsync(symbol);
        return Ok();
    }
}
