using EkubApi.DTOs;
using EkubApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EkubApi.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : BaseController
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Overview metrics for the Admin Dashboard.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AdminStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminStatsDto>> GetStats()
    {
        var adminId = GetUserId();
        var stats = await _adminService.GetStatsAsync(adminId);
        return Ok(stats);
    }

    /// <summary>
    /// List all registered users with activity details.
    /// </summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(List<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers()
    {
        var adminId = GetUserId();
        var users = await _adminService.GetUsersAsync(adminId);
        return Ok(users);
    }

    /// <summary>
    /// Toggle Admin role for a user (grant/revoke).
    /// </summary>
    [HttpPut("users/{userId}/toggle-admin")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDto>> ToggleUserAdmin(int userId)
    {
        var adminId = GetUserId();
        var user = await _adminService.ToggleUserAdminAsync(adminId, userId);
        return Ok(user);
    }

    /// <summary>
    /// List all user success stories (both approved and pending moderation).
    /// </summary>
    [HttpGet("stories")]
    [ProducesResponseType(typeof(List<AdminStoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AdminStoryDto>>> GetStories()
    {
        var adminId = GetUserId();
        var stories = await _adminService.GetStoriesAsync(adminId);
        return Ok(stories);
    }

    /// <summary>
    /// Approve a success story to make it public.
    /// </summary>
    [HttpPut("stories/{storyId}/approve")]
    [ProducesResponseType(typeof(AdminStoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminStoryDto>> ApproveStory(int storyId)
    {
        var adminId = GetUserId();
        var story = await _adminService.ApproveStoryAsync(adminId, storyId);
        return Ok(story);
    }

    /// <summary>
    /// Delete/reject a success story.
    /// </summary>
    [HttpDelete("stories/{storyId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStory(int storyId)
    {
        var adminId = GetUserId();
        await _adminService.DeleteStoryAsync(adminId, storyId);
        return NoContent();
    }

    /// <summary>
    /// List all submitted user inquiries and feedbacks.
    /// </summary>
    [HttpGet("feedbacks")]
    [ProducesResponseType(typeof(List<AdminFeedbackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<AdminFeedbackDto>>> GetFeedbacks()
    {
        var adminId = GetUserId();
        var feedbacks = await _adminService.GetFeedbacksAsync(adminId);
        return Ok(feedbacks);
    }

    /// <summary>
    /// Update feedback review status (Pending / Reviewed).
    /// </summary>
    [HttpPut("feedbacks/{feedbackId}/status")]
    [ProducesResponseType(typeof(AdminFeedbackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminFeedbackDto>> UpdateFeedbackStatus(int feedbackId, [FromBody] UpdateFeedbackStatusDto dto)
    {
        var adminId = GetUserId();
        var feedback = await _adminService.UpdateFeedbackStatusAsync(adminId, feedbackId, dto.Status);
        return Ok(feedback);
    }
}
