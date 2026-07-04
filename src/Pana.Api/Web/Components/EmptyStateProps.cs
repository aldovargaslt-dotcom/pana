namespace Pana.Api.Web.Components;

public record EmptyStateProps(
    string Title,
    string Description,
    string? ActionLabel = null,
    string? ActionHxGet = null,
    string? ActionHxTarget = "#modal-container"
);
