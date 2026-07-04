using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Analytics;

namespace Pana.Api.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Produces("application/json")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Profit & Loss (P&L) report for a date range.
    /// Calculates COGS from recipes linked to sold products.
    /// </summary>
    [HttpGet("profit-loss")]
    [ProducesResponseType(typeof(PlSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfitLoss(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        if (from > to) return BadRequest("'from' must be before 'to'.");
        if ((to - from).TotalDays > 365) return BadRequest("Date range cannot exceed 365 days.");

        var result = await _analyticsService.GetProfitLossAsync(from, to, ct);
        return Ok(result);
    }

    /// <summary>
    /// Waste/merma analysis for a date range.
    /// Shows production vs. waste per product with cost impact.
    /// </summary>
    [HttpGet("waste")]
    [ProducesResponseType(typeof(WasteAnalysisDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWasteAnalysis(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        if (from > to) return BadRequest("'from' must be before 'to'.");
        if ((to - from).TotalDays > 365) return BadRequest("Date range cannot exceed 365 days.");

        var result = await _analyticsService.GetWasteAnalysisAsync(from, to, ct);
        return Ok(result);
    }

    /// <summary>
    /// Sales trends with daily breakdown and payment method summary.
    /// </summary>
    [HttpGet("sales-trends")]
    [ProducesResponseType(typeof(SalesTrendsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalesTrends(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        if (from > to) return BadRequest("'from' must be before 'to'.");
        if ((to - from).TotalDays > 365) return BadRequest("Date range cannot exceed 365 days.");

        var result = await _analyticsService.GetSalesTrendsAsync(from, to, ct);
        return Ok(result);
    }
}
