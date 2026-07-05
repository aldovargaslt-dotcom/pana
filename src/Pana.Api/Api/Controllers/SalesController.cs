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
    /// Mark sale as being produced (in production).
    /// </summary>
    [HttpPost("{id:guid}/start-production")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartProduction(Guid id, CancellationToken ct)
    {
        var result = await _salesService.StartProductionAsync(id, ct);
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
    /// Deliver the sale (triggers inventory deduction).
    /// </summary>
    [HttpPost("{id:guid}/deliver")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deliver(Guid id, CancellationToken ct)
    {
        var result = await _salesService.DeliverAsync(id, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Cancel a sale.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _salesService.CancelAsync(id, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Record a payment against a sale.
    /// </summary>
    [HttpPost("{id:guid}/record-payment")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordPayment(Guid id, [FromBody] RecordPaymentRequest request, CancellationToken ct)
    {
        var result = await _salesService.RecordPaymentAsync(id, request.Amount, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Update pre-order details on a draft sale.
    /// </summary>
    [HttpPut("{id:guid}/pre-order-details")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePreOrderDetails(Guid id, [FromBody] UpdatePreOrderRequest request, CancellationToken ct)
    {
        var result = await _salesService.UpdatePreOrderDetailsAsync(id, request, ct);
        return result ? NoContent() : NotFound();
    }

    /// <summary>
    /// Get all active pre-orders (not delivered/cancelled).
    /// </summary>
    [HttpGet("pre-orders")]
    [ProducesResponseType(typeof(List<SaleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreOrders(CancellationToken ct)
    {
        var preOrders = await _salesService.GetPreOrdersAsync(ct);
        return Ok(preOrders);
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
