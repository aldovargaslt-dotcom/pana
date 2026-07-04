using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Export;

namespace Pana.Api.Api.Controllers;

[ApiController]
[Route("api/export")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportController(IExportService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>
    /// Export sales as CSV for a date range.
    /// </summary>
    [HttpGet("sales")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportSales(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        if (from > to) return BadRequest("'from' must be before 'to'.");

        var csv = await _exportService.ExportSalesCsvAsync(from, to, ct);
        return File(csv, "text/csv", $"ventas_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");
    }

    /// <summary>
    /// Export all active raw materials as CSV.
    /// </summary>
    [HttpGet("materials")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportMaterials(CancellationToken ct)
    {
        var csv = await _exportService.ExportMaterialsCsvAsync(ct);
        return File(csv, "text/csv", $"materias_primas_{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>
    /// Export P&L report as CSV for a date range.
    /// </summary>
    [HttpGet("profit-loss")]
    [Produces("text/csv")]
    public async Task<IActionResult> ExportProfitLoss(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct)
    {
        if (from > to) return BadRequest("'from' must be before 'to'.");

        var csv = await _exportService.ExportPlCsvAsync(from, to, ct);
        return File(csv, "text/csv", $"pl_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");
    }
}
