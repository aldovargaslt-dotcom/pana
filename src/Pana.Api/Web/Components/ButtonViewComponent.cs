using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Web.Components;

public class ButtonViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string text,
        string variant = "primary",
        string? icon = null,
        string? hxGet = null,
        string? hxPost = null,
        string? hxTarget = null,
        string? hxConfirm = null,
        bool disabled = false,
        string size = "md",
        string type = "button")
    {
        var props = new ButtonProps(text, variant, icon, hxGet, hxPost, hxTarget, hxConfirm, disabled, size, type);
        return View(props);
    }
}
