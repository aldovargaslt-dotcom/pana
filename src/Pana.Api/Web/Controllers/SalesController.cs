using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pana.Api.Application.Sales;

namespace Pana.Api.Web.Controllers;

/// <summary>
/// /sales redirects to /activity (unified orders view).
/// The /sales/{id} endpoint is kept to serve detail modals via HTMX.
/// </summary>
[Authorize]
[Route("sales")]
public class SalesController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => Redirect("/activity");

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
