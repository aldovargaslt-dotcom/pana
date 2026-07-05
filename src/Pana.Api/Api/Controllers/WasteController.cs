using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pana.Api.Application.Inventory;
using Pana.Api.Domain.Inventory;
using Pana.Api.Infrastructure.Data;

namespace Pana.Api.Api.Controllers;

[Route("api/waste")]
[ApiController]
[Authorize]
public class WasteController : BaseApiController
{
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(
        [FromServices] IWasteCategoryService service,
        CancellationToken ct)
    {
        var cats = await service.GetAllAsync(ct);
        return Ok(cats);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(
        [FromServices] IWasteCategoryService service,
        [FromBody] WasteCategoryCreateRequest request,
        CancellationToken ct)
    {
        var cat = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCategories), new { id = cat.Id }, cat);
    }

    [HttpPut("categories/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(
        [FromServices] IWasteCategoryService service,
        Guid id,
        [FromBody] WasteCategoryUpdateRequest request,
        CancellationToken ct)
    {
        var cat = await service.UpdateAsync(id, request, ct);
        if (cat is null) return NotFound();
        return Ok(cat);
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(
        [FromServices] IWasteCategoryService service,
        Guid id,
        CancellationToken ct)
    {
        var deleted = await service.DeleteAsync(id, ct);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPost("record")]
    public async Task<IActionResult> RecordWaste(
        [FromServices] PanaDbContext db,
        [FromBody] RecordWasteRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // TODO: from tenant context

        var movement = new InventoryMovement(
            tenantId,
            request.ProductId,
            "StockOut",
            -Math.Abs(request.Quantity),
            request.Reason ?? "Desperdicio registrado",
            performedByUserId: userId ?? Guid.Empty,
            wasteCategoryId: request.WasteCategoryId,
            wasteSubcategoryId: request.WasteSubcategoryId);

        db.InventoryMovements.Add(movement);
        await db.SaveChangesAsync(ct);

        return Ok(new
        {
            movement.Id,
            movement.ProductId,
            movement.Quantity,
            movement.MovementType,
            movement.Reason,
            movement.CreatedAt
        });
    }

    [HttpGet("records")]
    public async Task<IActionResult> GetWasteRecords(
        [FromServices] PanaDbContext db,
        [FromQuery] Guid? productId,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var query = db.InventoryMovements
            .Where(m => m.WasteCategoryId != null)
            .OrderByDescending(m => m.CreatedAt);

        if (productId.HasValue)
            query = (IOrderedQueryable<InventoryMovement>)query.Where(m => m.ProductId == productId.Value);

        var records = await query
            .Take(limit)
            .Select(m => new
            {
                m.Id,
                m.ProductId,
                m.Quantity,
                m.MovementType,
                m.Reason,
                m.WasteCategoryId,
                m.WasteSubcategoryId,
                m.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(records);
    }
}

public record RecordWasteRequest(
    Guid ProductId,
    decimal Quantity,
    Guid? WasteCategoryId = null,
    Guid? WasteSubcategoryId = null,
    string? Reason = null
);
