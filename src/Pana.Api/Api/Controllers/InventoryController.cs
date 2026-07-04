using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Inventory;

namespace Pana.Api.Api.Controllers;

[ApiController]
[Route("api/inventory")]
[Produces("application/json")]
[Authorize]
public class InventoryController : BaseApiController
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    /// <summary>
    /// Get current stock levels for all products.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<StockLevelDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStockLevels(CancellationToken ct)
    {
        var levels = await _inventoryService.GetStockLevelsAsync(ct);
        return Ok(levels);
    }

    /// <summary>
    /// Get recent inventory movements, optionally filtered by product.
    /// </summary>
    [HttpGet("movements")]
    [ProducesResponseType(typeof(List<InventoryMovementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMovements([FromQuery] Guid? productId = null, [FromQuery] int limit = 50, CancellationToken ct = default)
    {
        var movements = await _inventoryService.GetMovementsAsync(productId, limit, ct);
        return Ok(movements);
    }

    /// <summary>
    /// Record stock coming in (purchase, production, return).
    /// </summary>
    [HttpPost("stock-in")]
    [ProducesResponseType(typeof(InventoryMovementDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> StockIn([FromBody] StockInRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var movement = await _inventoryService.StockInAsync(request, userId, ct);
        return CreatedAtAction(nameof(GetMovements), new { productId = request.ProductId }, movement);
    }

    /// <summary>
    /// Record stock going out (waste, transfer, damage).
    /// </summary>
    [HttpPost("stock-out")]
    [ProducesResponseType(typeof(InventoryMovementDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> StockOut([FromBody] StockOutRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var movement = await _inventoryService.StockOutAsync(request, userId, ct);
        return CreatedAtAction(nameof(GetMovements), new { productId = request.ProductId }, movement);
    }

    /// <summary>
    /// Adjust stock to a specific level (inventory count correction).
    /// </summary>
    [HttpPost("adjust")]
    [ProducesResponseType(typeof(InventoryMovementDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Adjust([FromBody] AdjustmentRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var movement = await _inventoryService.AdjustAsync(request, userId, ct);
        return Ok(movement);
    }
}
