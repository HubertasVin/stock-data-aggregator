using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockDataAggregator.Application.Interfaces;
using StockDataAggregator.Application.Services;

namespace StockDataAggregator.Api.Controllers;

[ApiController]
[Route("api/v1/symbols")]
public sealed class TrackedSymbolsController : ControllerBase
{
    private readonly ISymbolMetricsRepository _repo;
    private readonly TrackedSymbolsService _trackedSymbolsService;

    public TrackedSymbolsController(
        ISymbolMetricsRepository repo,
        TrackedSymbolsService trackedSymbolsService
    )
    {
        _repo = repo;
        _trackedSymbolsService = trackedSymbolsService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> Get() =>
        Ok(await _repo.GetAllSymbolsAsync());

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Post([FromBody] string symbol)
    {
        if (!await _trackedSymbolsService.CheckIfYahooHasSymbolAsync(symbol))
            return NotFound();

        await _repo.AddSymbolAsync(symbol);
        return Ok();
    }
}
