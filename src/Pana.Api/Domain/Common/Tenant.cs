namespace Pana.Api.Domain.Common;

/// <summary>
/// Represents a tenant (business) in the platform.
/// </summary>
public class Tenant : BaseEntity
{
    public string Name { get; private set; }
    public string Slug { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Tenant() { } // EF Core

    public Tenant(string name, string slug)
    {
        SetName(name);
        SetSlug(slug);
    }

    public Tenant(Guid id, string name, string slug) : base(id)
    {
        SetName(name);
        SetSlug(slug);
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required.", nameof(name));
        Name = name.Trim();
        MarkUpdated();
    }

    public void SetSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Tenant slug is required.", nameof(slug));
        Slug = slug.Trim().ToLowerInvariant();
        MarkUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }
}
