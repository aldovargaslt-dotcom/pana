namespace Pana.Api.Infrastructure.Data;

/// <summary>
/// Provides the current tenant context from the HTTP request.
/// The tenant is resolved from the request (header, subdomain, or JWT claim).
/// For the initial bakery implementation, this returns a hardcoded tenant.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
}

public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Hardcoded tenant for initial single-tenant bakery setup.
    // When multi-tenancy goes live, resolve from request.
    private static readonly Guid DefaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            // Future: resolve from X-Tenant-Id header, subdomain, or JWT claim
            var header = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (Guid.TryParse(header, out var tenantId))
                return tenantId;

            return DefaultTenantId;
        }
    }
}
