using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Products;

namespace Pana.Api.Api.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get all products for the current tenant.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var products = await _productService.GetAllAsync(ct);
        return Ok(products);
    }

    /// <summary>
    /// Get a product by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(id, ct);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>
    /// Create a new product.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ProductRequest request, CancellationToken ct)
    {
        var product = await _productService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>
    /// Update an existing product.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductRequest request, CancellationToken ct)
    {
        var product = await _productService.UpdateAsync(id, request, ct);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>
    /// Soft-delete a product (deactivates it).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _productService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
