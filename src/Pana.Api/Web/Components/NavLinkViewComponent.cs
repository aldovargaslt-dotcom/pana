using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.Components;

/// <summary>
/// Sidebar navigation link. Used from _Layout directly.
/// </summary>
public class NavLinkViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string href, string icon, string text)
    {
        var isActive = ViewContext.HttpContext.Request.Path == href
            || (href != "/" && ViewContext.HttpContext.Request.Path.StartsWithSegments(href));

        return View("Default", new NavLinkData(href, icon, text, isActive));
    }
}

public record NavLinkData(string Href, string Icon, string Text, bool IsActive);
