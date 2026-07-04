using Microsoft.AspNetCore.Mvc;

namespace Pana.Api.Api.Controllers;

/// <summary>
/// Base controller with shared helpers for all API controllers.
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Extracts the current user's ID from the JWT claims.
    /// </summary>
    protected Guid? GetCurrentUserId()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userId, out var id) ? id : null;
    }
}
