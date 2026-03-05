using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace UniStart.Controllers;

/// <summary>
/// Base controller with shared user-identity helper.
/// All authenticated API controllers should inherit from this.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        if (int.TryParse(claim, out var id) && id > 0) return id;
        throw new UnauthorizedAccessException("Invalid user identity");
    }
}
