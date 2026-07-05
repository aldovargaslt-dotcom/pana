using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Products;
using Pana.Api.Application.Sales;
using Pana.Api.Web.ViewModels;

namespace Pana.Api.Web.Controllers;

[Route("sales")]
public class SalesController : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromServices] ISalesService salesService,
        CancellationToken ct)
    {
        var sales = await salesService.GetAllAsync(ct);
        var vm = sales.Select(s => new SaleRowViewModel(
            s.Id, s.Status, s.TotalAmount, s.Items.Count, s.CreatedAt,
            s.OrderType, s.CustomerName, s.PaymentStatus, s.ScheduledDate
        )).ToList();

        ViewData["Title"] = "Ventas";
        ViewData["ActiveNav"] = "sales";

        return View(new SaleListViewModel(vm, vm.Count));
    }

    [HttpGet("table-rows")]
    public async Task<IActionResult> TableRows(
        [FromServices] ISalesService salesService,
        CancellationToken ct)
    {
        var sales = await salesService.GetAllAsync(ct);
        var rows = sales.Select(s => new SaleRowViewModel(
            s.Id, s.Status, s.TotalAmount, s.Items.Count, s.CreatedAt,
            s.OrderType, s.CustomerName, s.PaymentStatus, s.ScheduledDate
        )).ToList();

        return PartialView("_TableRows", rows);
    }

    [HttpGet("create-form")]
    public async Task<IActionResult> CreateForm(
        [FromServices] IProductService productService,
        CancellationToken ct)
    {
        var products = await productService.GetAllAsync(ct);
        ViewBag.Products = products
            .Where(p => p.IsActive)
            .Select(p =>
            {
                dynamic obj = new System.Dynamic.ExpandoObject();
                obj.Id = p.Id;
                obj.Name = p.Name;
                obj.Price = p.Price;
                return obj;
            })
            .ToList();

        return PartialView("_Form", new SaleFormViewModel());
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(
        [FromServices] ISalesService salesService,
        [FromForm] SaleFormViewModel form,
        CancellationToken ct)
    {
        if (!ModelState.IsValid || form.Items == null || form.Items.Count == 0)
        {
            if (form.Items == null || form.Items.Count == 0)
                ModelState.AddModelError("Items", "Agregá al menos un producto.");

            Response.Headers["HX-Retarget"] = "#modal-container";

            // Reload products for the form
            var productService = HttpContext.RequestServices.GetRequiredService<IProductService>();
            var products = await productService.GetAllAsync(ct);
            ViewBag.Products = products
                .Where(p => p.IsActive)
                .Select(p =>
                {
                    dynamic obj = new System.Dynamic.ExpandoObject();
                    obj.Id = p.Id;
                    obj.Name = p.Name;
                    obj.Price = p.Price;
                    return obj;
                })
                .ToList();

            return PartialView("_Form", form);
        }

        var request = new CreateSaleRequest(
            form.Items
                .Where(i => i.ProductId != Guid.Empty && i.Quantity > 0)
                .Select(i => new CreateSaleItemRequest(i.ProductId, i.UnitPrice, i.Quantity))
                .ToList(),
            form.Notes
        );

        await salesService.CreateAsync(request, soldByUserId: null, ct: ct);

        Response.Headers["HX-Trigger"] = "sale-created";
        // Reload the table
        return await TableRows(salesService, ct);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detail(
        Guid id,
        [FromServices] ISalesService salesService,
        CancellationToken ct)
    {
        var sale = await salesService.GetByIdAsync(id, ct);
        if (sale is null) return NotFound();

        return PartialView("_Detail", sale);
    }
}
