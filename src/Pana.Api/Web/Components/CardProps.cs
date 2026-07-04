namespace Pana.Api.Web.Components;

public record CardProps(
    string? Title = null,
    string? Subtitle = null,
    string? HxGet = null,
    string? HxTrigger = "load",
    string Padding = "md"
)
{
    public string PaddingClasses => Padding switch
    {
        "sm" => "p-3",
        "lg" => "p-6",
        _ => "p-4"
    };
}
