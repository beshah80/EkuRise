using EkubApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EkubApi.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// List all notifications for the current user (newest first).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = GetUserId();
        var notifications = await _notificationService.GetNotificationsAsync(userId);
        return Ok(notifications);
    }

    /// <summary>
    /// Get the count of unread notifications (for the badge).
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(new { count });
    }

    /// <summary>
    /// Mark a single notification as read.
    /// </summary>
    [HttpPut("{notificationId}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var userId = GetUserId();
        await _notificationService.MarkAsReadAsync(userId, notificationId);
        return Ok(new { message = "Marked as read." });
    }

    /// <summary>
    /// Mark all notifications as read.
    /// </summary>
    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = GetUserId();
        await _notificationService.MarkAllReadAsync(userId);
        return Ok(new { message = "All notifications marked as read." });
    }
}
