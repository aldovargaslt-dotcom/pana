using Microsoft.AspNetCore.Mvc;
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
            s.Id, s.Status, s.TotalAmount, s.Items.Count, s.CreatedAt
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
            s.Id, s.Status, s.TotalAmount, s.Items.Count, s.CreatedAt
        )).ToList();

        return PartialView("_TableRows", rows);
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
