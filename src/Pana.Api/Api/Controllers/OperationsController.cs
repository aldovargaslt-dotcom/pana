using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Operations;

namespace Pana.Api.Api.Controllers;

[ApiController]
[Route("api/operations")]
[Produces("application/json")]
[Authorize]
public class OperationsController : BaseApiController
{
    private readonly IProductionCaptureService _captureService;

    public OperationsController(IProductionCaptureService captureService)
    {
        _captureService = captureService;
    }

    /// <summary>
    /// Get today's production capture (initial, production, returns per product).
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(DailyProductionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetToday(CancellationToken ct)
    {
        var production = await _captureService.GetTodayAsync(ct);
        return production is null ? Ok(new { message = "No capture yet for today." }) : Ok(production);
    }

    /// <summary>
    /// Get production capture for a specific date.
    /// </summary>
    [HttpGet("{date:datetime}")]
    [ProducesResponseType(typeof(DailyProductionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByDate(DateTime date, CancellationToken ct)
    {
        var production = await _captureService.GetByDateAsync(date, ct);
        return production is null ? NotFound() : Ok(production);
    }

    /// <summary>
    /// Create or update today's production capture. Creates DailyContext if needed.
    /// </summary>
    [HttpPut("today")]
    [ProducesResponseType(typeof(DailyProductionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertToday([FromBody] UpsertDailyProductionRequest request, CancellationToken ct)
    {
        var production = await _captureService.UpsertTodayAsync(request, ct);
        return Ok(production);
    }

    /// <summary>
    /// Close today's production. Generates InventoryMovements for production and waste.
    /// </summary>
    [HttpPost("today/close")]
    [ProducesResponseType(typeof(DailyProductionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CloseToday(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var production = await _captureService.CloseTodayAsync(userId.Value, ct);
        return Ok(production);
    }

    /// <summary>
    /// Reopen today's production if it was closed by mistake.
    /// </summary>
    [HttpPost("today/reopen")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReopenToday(CancellationToken ct)
    {
        await _captureService.ReopenTodayAsync(ct);
        return NoContent();
    }
}
