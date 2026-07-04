using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Pana.Api.Web.TagHelpers;

[HtmlTargetElement("pana-badge")]
public class BadgeTagHelper : TagHelper
{
    public string Variant { get; set; } = "default";

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";

        var variantClass = Variant switch
        {
            "success" => "bg-emerald-100 text-emerald-700",
            "warning" => "bg-amber-100 text-amber-700",
            "danger" => "bg-red-100 text-red-700",
            "info" => "bg-blue-100 text-blue-700",
            _ => "bg-gray-100 text-gray-700"
        };

        output.Attributes.SetAttribute("class", $"inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {variantClass}");

        var childContent = (await output.GetChildContentAsync()).GetContent();
        output.Content.SetHtmlContent(childContent);
    }
}
