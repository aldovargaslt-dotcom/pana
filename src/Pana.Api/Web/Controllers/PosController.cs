using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Products;
using Pana.Api.Application.Sales;
using Pana.Api.Web.ViewModels;

namespace Pana.Api.Web.Controllers;

[Authorize]
[Route("pos")]
public class PosController : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromServices] IProductService productService,
        [FromServices] ISalesService salesService,
        CancellationToken ct)
    {
        var products = await productService.GetAllAsync(ct);
        var activeProducts = products.Where(p => p.IsActive).ToList();

        // Build categories from ProductType groupings
        var categoryMap = activeProducts
            .GroupBy(p => p.ProductType)
            .ToDictionary(g => g.Key, g => g.ToList());

        var categories = new List<PosCategoryViewModel>();
        foreach (var (productType, prods) in categoryMap)
        {
            var (name, icon) = productType switch
            {
                "Storable" => ("Productos", "box"),
                "Consumable" => ("Consumibles", "coffee"),
                "Service" => ("Servicios", "briefcase"),
                _ => (productType, "package")
            };
            categories.Add(new PosCategoryViewModel(name, icon, prods.Count));
        }

        var productCards = activeProducts.Select(p => new PosProductCardViewModel(
            p.Id, p.Name, p.Description, p.Sku, p.Price, p.ProductType, p.IsActive
        )).ToList();

        ViewData["Title"] = "Punto de Venta";
        ViewData["ActiveNav"] = "pos";

        var vm = new PosViewModel(categories, productCards, null);
        return View(vm);
    }

    [HttpGet("product/{id:guid}")]
    public async Task<IActionResult> ProductDetail(
        Guid id,
        [FromServices] IProductService productService,
        CancellationToken ct)
    {
        var product = await productService.GetByIdAsync(id, ct);
        if (product is null) return NotFound();

        var detail = new PosProductDetailViewModel(
            product.Id, product.Name, product.Description, product.Sku,
            product.Price, product.ProductType, product.IsActive);

        return PartialView("_ProductDetailModal", detail);
    }

    [HttpGet("order-panel")]
    public IActionResult OrderPanel()
    {
        return PartialView("_OrderPanel", new PosActiveOrderViewModel(
            null, null, null, "Standard", new(), 0, 0, 0, 0, null, "Efectivo",
            null, null, null, 0, null));
    }

    [HttpPost("place-order")]
    public async Task<IActionResult> PlaceOrder(
        [FromServices] ISalesService salesService,
        [FromForm] PosPlaceOrderRequest request,
        CancellationToken ct)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            Response.Headers["HX-Retarget"] = "#order-panel";
            return PartialView("_OrderPanel", new PosActiveOrderViewModel(
                null, null, null, "Standard", new(), 0, 0, 0, 0, null, "Efectivo",
                null, null, null, 0, null));
        }

        var saleRequest = new CreateSaleRequest(
            request.Items
                .Where(i => i.ProductId != Guid.Empty && i.Quantity > 0)
                .Select(i => new CreateSaleItemRequest(i.ProductId, i.UnitPrice, i.Quantity))
                .ToList(),
            request.Notes,
            request.OrderType ?? "Standard",
            request.CustomerName,
            request.CustomerPhone,
            request.ScheduledDate,
            request.DepositAmount ?? 0,
            request.PaymentMethod,
            request.InternalNotes
        );

        var sale = await salesService.CreateAsync(saleRequest, soldByUserId: null, ct: ct);

        // Standard orders are confirmed immediately; pre-orders stay in Draft for staff review
        if (sale.OrderType != "PreOrder")
            await salesService.ConfirmAsync(sale.Id, ct);

        Response.Headers["HX-Trigger"] = "order-placed";
        return PartialView("_OrderConfirmation", sale);
    }
}

// Form binding model for place-order
public class PosPlaceOrderRequest
{
    public List<PosPlaceOrderItem>? Items { get; set; }
    public string? Notes { get; set; }
    public string? OrderType { get; set; }
    public string? PaymentMethod { get; set; }
    // Pre-order fields
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public decimal? DepositAmount { get; set; }
    public string? InternalNotes { get; set; }
}

public class PosPlaceOrderItem
{
    public Guid ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
