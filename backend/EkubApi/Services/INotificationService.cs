using EkubApi.DTOs;
using EkubApi.Enums;

namespace EkubApi.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetNotificationsAsync(int userId);
    Task MarkAsReadAsync(int userId, int notificationId);
    Task MarkAllReadAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);

    /// <summary>
    /// Internal method called by other services to create notifications.
    /// </summary>
    Task CreateAsync(int userId, NotificationType type, string title, string body, int? circleId = null, int? roundId = null);
}
