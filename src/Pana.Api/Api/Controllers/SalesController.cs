using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Sales;

namespace Pana.Api.Api.Controllers;

[ApiController]
[Route("api/sales")]
[Produces("application/json")]
[Authorize]
public class SalesController : BaseApiController
{
    private readonly ISalesService _salesService;

    public SalesController(ISalesService salesService)
    {
        _salesService = salesService;
    }

    /// <summary>
    /// Get all sales for the current tenant, newest first.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SaleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var sales = await _salesService.GetAllAsync(ct);
        return Ok(sales);
    }

    /// <summary>
    /// Get a specific sale by ID with all line items.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var sale = await _salesService.GetByIdAsync(id, ct);
        return sale is null ? NotFound() : Ok(sale);
    }

    /// <summary>
    /// Create a new sale transaction (starts as Draft).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var sale = await _salesService.CreateAsync(request, userId, ct);
        return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
    }

    /// <summary>
    /// Confirm a draft sale.
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var result = await _salesService.ConfirmAsync(id, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Mark sale as being prepared.
    /// </summary>
    [HttpPost("{id:guid}/start-preparing")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartPreparing(Guid id, CancellationToken ct)
    {
        var result = await _salesService.StartPreparingAsync(id, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Mark sale as ready for delivery/pickup.
    /// </summary>
    [HttpPost("{id:guid}/mark-ready")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkReady(Guid id, CancellationToken ct)
    {
        var result = await _salesService.MarkReadyAsync(id, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Complete the sale (triggers inventory deduction).
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        var result = await _salesService.CompleteAsync(id, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Void a sale (cancels the transaction).
    /// </summary>
    [HttpPost("{id:guid}/void")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Void(Guid id, CancellationToken ct)
    {
        var result = await _salesService.VoidAsync(id, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Get today's sales summary (revenue, count, margin).
    /// </summary>
    [HttpGet("summary/today")]
    [ProducesResponseType(typeof(DailySalesSummary), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodaySummary(CancellationToken ct)
    {
        var summary = await _salesService.GetDailySummaryAsync(null, ct);
        return Ok(summary);
    }

    /// <summary>
    /// Get sales summary for a specific date.
    /// </summary>
    [HttpGet("summary/{date:datetime}")]
    [ProducesResponseType(typeof(DailySalesSummary), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDateSummary(DateTime date, CancellationToken ct)
    {
        var summary = await _salesService.GetDailySummaryAsync(date, ct);
        return Ok(summary);
    }
}
