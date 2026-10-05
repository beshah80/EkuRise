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
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        return id;
    }
}
