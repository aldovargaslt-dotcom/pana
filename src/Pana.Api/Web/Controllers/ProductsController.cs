using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Products;
using Pana.Api.Web.ViewModels;

namespace Pana.Api.Web.Controllers;

[Authorize(Roles = "Admin")]
[Route("products")]
public class ProductsController : Controller
{
    private const string UploadsDir = "wwwroot/uploads/products";

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromServices] IProductService productService,
        CancellationToken ct)
    {
        var products = await productService.GetAllAsync(ct);
        var vm = products.Select(p => new ProductRowViewModel(
            p.Id, p.Name, p.Sku, p.Price, p.Cost, p.Margin, p.MarginPercentage, p.IsActive, p.CreatedAt, p.ImageUrl
        )).ToList();

        ViewData["Title"] = "Productos";
        ViewData["ActiveNav"] = "products";

        return View(new ProductListViewModel(vm, vm.Count));
    }

    [HttpGet("table-rows")]
    public async Task<IActionResult> TableRows(
        [FromServices] IProductService productService,
        [FromQuery] string? q,
        CancellationToken ct)
    {
        var products = await productService.GetAllAsync(ct);
        var rows = products.Select(p => new ProductRowViewModel(
            p.Id, p.Name, p.Sku, p.Price, p.Cost, p.Margin, p.MarginPercentage, p.IsActive, p.CreatedAt, p.ImageUrl
        ));

        if (!string.IsNullOrWhiteSpace(q))
        {
            rows = rows.Where(r =>
                r.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                r.Sku.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        return PartialView("_TableRows", rows.ToList());
    }

    [HttpGet("create-form")]
    public IActionResult CreateForm()
    {
        return PartialView("_Form", new ProductFormViewModel());
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromServices] IProductService productService,
        [FromForm] ProductFormViewModel form,
        IFormFile? imageFile,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            Response.Headers["HX-Retarget"] = "#modal-container";
            return PartialView("_Form", form);
        }

        var imageUrl = await SaveImageAsync(imageFile);
        var request = new ProductRequest
        {
            Name = form.Name, Sku = form.Sku, Price = form.Price, Cost = form.Cost,
            Description = form.Description, ImageUrl = imageUrl
        };
        await productService.CreateAsync(request, ct);

        Response.Headers["HX-Trigger"] = "product-created";
        return await TableRows(productService, null, ct);
    }

    [HttpGet("{id:guid}/edit-form")]
    public async Task<IActionResult> EditForm(
        Guid id,
        [FromServices] IProductService productService,
        CancellationToken ct)
    {
        var product = await productService.GetByIdAsync(id, ct);
        if (product is null) return NotFound();

        var form = new ProductFormViewModel(
            product.Id, product.Name, product.Sku,
            product.Price, product.Cost, product.Description, product.ImageUrl);

        return PartialView("_Form", form);
    }

    [HttpPost("{id:guid}/edit")]
    public async Task<IActionResult> Edit(
        Guid id,
        [FromServices] IProductService productService,
        [FromForm] ProductFormViewModel form,
        IFormFile? imageFile,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            Response.Headers["HX-Retarget"] = "#modal-container";
            return PartialView("_Form", form);
        }

        var imageUrl = await SaveImageAsync(imageFile) ?? form.ImageUrl;
        var request = new ProductRequest
        {
            Name = form.Name, Sku = form.Sku, Price = form.Price, Cost = form.Cost,
            Description = form.Description, ImageUrl = imageUrl
        };
        await productService.UpdateAsync(id, request, ct);

        var product = await productService.GetByIdAsync(id, ct);
        var row = new ProductRowViewModel(
            product!.Id, product.Name, product.Sku, product.Price,
            product.Cost, product.Margin, product.MarginPercentage,
            product.IsActive, product.CreatedAt, product.ImageUrl);

        return PartialView("_TableRow", row);
    }

    [HttpPost("{id:guid}/delete")]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromServices] IProductService productService,
        CancellationToken ct)
    {
        await productService.DeleteAsync(id, ct);
        return Ok();
    }

    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
            return null;

        Directory.CreateDirectory(UploadsDir);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(UploadsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/products/{fileName}";
    }
}
