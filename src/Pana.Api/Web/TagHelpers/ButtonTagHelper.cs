using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Pana.Api.Web.TagHelpers;

[HtmlTargetElement("pana-button")]
public class ButtonTagHelper : TagHelper
{
    public string Variant { get; set; } = "primary";
    public string? HxGet { get; set; }
    public string? HxPost { get; set; }
    public string? HxTarget { get; set; }
    public string? HxConfirm { get; set; }
    public string? Icon { get; set; }
    public string Size { get; set; } = "md";
    public bool Disabled { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "button";
        output.Attributes.SetAttribute("type", "button");

        // Variant classes
        var variantClass = Variant switch
        {
            "primary" => "bg-brand-600 text-white hover:bg-brand-700 focus:ring-brand-500",
            "secondary" => "bg-white text-gray-700 border border-gray-300 hover:bg-gray-50 focus:ring-brand-500",
            "danger" => "bg-red-600 text-white hover:bg-red-700 focus:ring-red-500",
            "ghost" => "text-gray-600 hover:bg-gray-100 focus:ring-gray-500",
            "success" => "bg-emerald-600 text-white hover:bg-emerald-700 focus:ring-emerald-500",
            _ => "bg-brand-600 text-white hover:bg-brand-700 focus:ring-brand-500"
        };

        var sizeClass = Size switch
        {
            "sm" => "px-3 py-1.5 text-sm",
            "lg" => "px-5 py-2.5 text-base",
            _ => "px-4 py-2 text-sm"
        };

        var baseClass = "inline-flex items-center justify-center gap-2 rounded-lg font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-offset-1";
        if (Disabled)
            baseClass += " opacity-50 cursor-not-allowed";
        else
            baseClass += " cursor-pointer";

        output.Attributes.SetAttribute("class", $"{baseClass} {variantClass} {sizeClass}");

        // HTMX attributes
        if (HxGet != null) output.Attributes.SetAttribute("hx-get", HxGet);
        if (HxPost != null) output.Attributes.SetAttribute("hx-post", HxPost);
        if (HxTarget != null) output.Attributes.SetAttribute("hx-target", HxTarget);
        if (HxConfirm != null) output.Attributes.SetAttribute("hx-confirm", HxConfirm);
        if (Disabled) output.Attributes.SetAttribute("disabled", "");

        // Icon
        var childContent = (await output.GetChildContentAsync()).GetContent();
        if (Icon != null)
        {
            output.Content.SetHtmlContent($"<i class=\"{Icon}\"></i> {childContent}");
        }
        else
        {
            output.Content.SetHtmlContent(childContent);
        }
    }
}
