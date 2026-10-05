using EkubApi.DTOs;
using EkubApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EkubApi.Controllers;

[ApiController]
[Route("api/circles")]
public class CirclesController : BaseController
{
    private readonly ICircleService _circleService;

    public CirclesController(ICircleService circleService)
    {
        _circleService = circleService;
    }

    /// <summary>
    /// Create a new circle. The creating user becomes the organizer and first member.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CircleDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CircleDetailDto>> Create([FromBody] CreateCircleDto dto)
    {
        var organizerId = GetUserId();
        var circle = await _circleService.CreateCircleAsync(dto, organizerId);
        return CreatedAtAction(nameof(GetById), new { circleId = circle.Id }, circle);
    }

    /// <summary>
    /// List all circles the current user is a member of.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CircleSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CircleSummaryDto>>> MyCircles()
    {
        var userId = GetUserId();
        var circles = await _circleService.GetMyCirclesAsync(userId);
        return Ok(circles);
    }

    /// <summary>
    /// Get full details of a circle: contribution, members, current round.
    /// Only visible to members of the circle.
    /// </summary>
    [HttpGet("{circleId}")]
    [ProducesResponseType(typeof(CircleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CircleDetailDto>> GetById(int circleId)
    {
        var userId = GetUserId();
        var circle = await _circleService.GetCircleByIdAsync(circleId, userId);
        return circle is null ? NotFound() : Ok(circle);
    }

    /// <summary>
    /// Add a member to a forming circle. The user must already be registered.
    /// Organizer only.
    /// </summary>
    [HttpPost("{circleId}/members")]
    [ProducesResponseType(typeof(MemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MemberDto>> AddMember(int circleId, [FromBody] AddMemberDto dto)
    {
        var organizerId = GetUserId();
        var member = await _circleService.AddMemberAsync(circleId, dto, organizerId);
        return Ok(member);
    }

    /// <summary>
    /// Remove a member from a forming circle. Organizer only.
    /// </summary>
    [HttpDelete("{circleId}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(int circleId, int userId)
    {
        var organizerId = GetUserId();
        await _circleService.RemoveMemberAsync(circleId, userId, organizerId);
        return NoContent();
    }

    /// <summary>
    /// Start the circle. Locks the member list, creates one round per member,
    /// and sets the fixed payout order. Organizer only.
    /// </summary>
    [HttpPost("{circleId}/start")]
    [ProducesResponseType(typeof(CircleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CircleDetailDto>> Start(int circleId)
    {
        var organizerId = GetUserId();
        var circle = await _circleService.StartCircleAsync(circleId, organizerId);
        return Ok(circle);
    }
}
