namespace Pana.Api.Web.Components;

public record ModalProps(
    string Id,
    string Title,
    string? HxGet = null,
    string? HxPost = null,
    string Size = "md"  // sm | md | lg
)
{
    public string SizeClasses => Size switch
    {
        "sm" => "max-w-md",
        "lg" => "max-w-3xl",
        _ => "max-w-2xl"
    };
}
