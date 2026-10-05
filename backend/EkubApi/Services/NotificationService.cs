using EkubApi.Data;
using EkubApi.DTOs;
using EkubApi.Entities;
using EkubApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace EkubApi.Services;

public class NotificationService : INotificationService
{
    private readonly EkubDbContext _db;

    public NotificationService(EkubDbContext db)
    {
        _db = db;
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync(int userId)
    {
        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return notifications.Select(MapToDto).ToList();
    }

    public async Task MarkAsReadAsync(int userId, int notificationId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId)
            ?? throw new KeyNotFoundException("Notification not found.");

        notification.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(int userId)
    {
        await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _db.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task CreateAsync(int userId, NotificationType type, string title, string body, int? circleId = null, int? roundId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            IsRead = false,
            RelatedCircleId = circleId,
            RelatedRoundId = roundId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();
    }

    private static NotificationDto MapToDto(Notification n) => new(
        n.Id,
        n.Type,
        n.Title,
        n.Body,
        n.IsRead,
        n.RelatedCircleId,
        n.RelatedRoundId,
        n.CreatedAt
    );
}
