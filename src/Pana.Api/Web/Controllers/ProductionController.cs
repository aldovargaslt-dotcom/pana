using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.Controllers;

/// <summary>
/// Redirects to /inventory — unified daily operations view.
/// </summary>
[Authorize]
[Route("production")]
public class ProductionController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => Redirect("/inventory");

    [HttpPost("upsert")]
    public IActionResult Upsert() => Redirect("/inventory");

    [HttpPost("close")]
    public IActionResult CloseDay() => Redirect("/inventory");
}
