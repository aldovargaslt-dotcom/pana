using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Inventory;

namespace Pana.Api.Api.Controllers;

[ApiController]
[Route("api/locations")]
[Produces("application/json")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly IStockLocationService _locationService;

    public LocationsController(IStockLocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// Get all stock locations.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<StockLocationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var locations = await _locationService.GetAllAsync(ct);
        return Ok(locations);
    }

    /// <summary>
    /// Get a specific location by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StockLocationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var location = await _locationService.GetByIdAsync(id, ct);
        return location is null ? NotFound() : Ok(location);
    }

    /// <summary>
    /// Create a new stock location (warehouse, zone, shelf, or bin).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(StockLocationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStockLocationRequest request, CancellationToken ct)
    {
        var location = await _locationService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = location.Id }, location);
    }

    /// <summary>
    /// Deactivate a stock location.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await _locationService.DeactivateAsync(id, ct);
        return result ? NoContent() : NotFound();
    }
}
