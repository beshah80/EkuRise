using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkubApi.Controllers;

/// <summary>
/// Base controller providing the current authenticated user's ID from JWT claims.
/// All protected controllers inherit from this.
/// </summary>
[ApiController]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected int GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (claim is null || !int.TryParse(claim, out var id))
        {
            // 401 Unauthorized: the token is absent or the identity claim is missing.
            // Throwing UnauthorizedAccessException would map to 403 via the middleware,
            // so we write the status code directly and throw a specific exception type.
            HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            throw new InvalidOperationException("User is not authenticated.");
        }

        return id;
    }
}
