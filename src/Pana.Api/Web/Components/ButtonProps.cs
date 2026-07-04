namespace Pana.Api.Web.Components;

public record ButtonProps(
    string Text,
    string Variant = "primary",
    string? Icon = null,
    string? HxGet = null,
    string? HxPost = null,
    string? HxTarget = null,
    string? HxConfirm = null,
    bool Disabled = false,
    string? Size = "md",
    string? Type = "button"
)
{
    public string CssClasses => Variant switch
    {
        "primary" => "bg-brand-600 text-white hover:bg-brand-700 focus:ring-brand-500",
        "secondary" => "bg-white text-gray-700 border border-gray-300 hover:bg-gray-50 focus:ring-brand-500",
        "danger" => "bg-red-600 text-white hover:bg-red-700 focus:ring-red-500",
        "ghost" => "text-gray-600 hover:bg-gray-100 focus:ring-gray-500",
        "success" => "bg-emerald-600 text-white hover:bg-emerald-700 focus:ring-emerald-500",
        _ => "bg-brand-600 text-white hover:bg-brand-700 focus:ring-brand-500"
    };

    public string SizeClasses => Size switch
    {
        "sm" => "px-3 py-1.5 text-sm",
        "lg" => "px-5 py-2.5 text-base",
        _ => "px-4 py-2 text-sm"
    };
}
