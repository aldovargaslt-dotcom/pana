using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Production;

namespace Pana.Api.Api.Controllers;

[ApiController]
[Route("api/materials")]
[Produces("application/json")]
[Authorize]
public class MaterialsController : ControllerBase
{
    private readonly IRawMaterialService _materialService;

    public MaterialsController(IRawMaterialService materialService)
    {
        _materialService = materialService;
    }

    /// <summary>
    /// Get all raw materials for the current tenant. Optionally filter by category.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RawMaterialDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? category, CancellationToken ct)
    {
        var materials = await _materialService.GetAllAsync(category, ct);
        return Ok(materials);
    }

    /// <summary>
    /// Get a raw material by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RawMaterialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var material = await _materialService.GetByIdAsync(id, ct);
        return material is null ? NotFound() : Ok(material);
    }

    /// <summary>
    /// Create a new raw material.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RawMaterialDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] RawMaterialRequest request, CancellationToken ct)
    {
        var material = await _materialService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = material.Id }, material);
    }

    /// <summary>
    /// Update an existing raw material.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RawMaterialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] RawMaterialRequest request, CancellationToken ct)
    {
        var material = await _materialService.UpdateAsync(id, request, ct);
        return material is null ? NotFound() : Ok(material);
    }

    /// <summary>
    /// Delete (deactivate) a raw material.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _materialService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
