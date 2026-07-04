namespace Pana.Api.Domain.Common;

/// <summary>
/// Base class for entities that belong to a specific tenant.
/// Multi-tenancy is enforced at the data layer via EF Core global query filter.
/// </summary>
public abstract class TenantEntity : BaseEntity
{
    public Guid TenantId { get; protected set; }

    protected TenantEntity() { }

    protected TenantEntity(Guid tenantId) : base()
    {
        TenantId = tenantId;
    }

    protected TenantEntity(Guid id, Guid tenantId) : base(id)
    {
        TenantId = tenantId;
    }
}
