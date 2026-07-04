using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.ViewComponents;

public class EmptyStateViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string message, string? icon = null, string? actionUrl = null, string? actionLabel = null)
    {
        ViewBag.Message = message;
        ViewBag.Icon = icon ?? "📭";
        ViewBag.ActionUrl = actionUrl;
        ViewBag.ActionLabel = actionLabel;
        return View();
    }
}
