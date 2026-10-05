using EkubApi.Enums;

namespace EkubApi.Entities;

/// <summary>
/// A notification shown in the user's notification bell.
/// Examples: payment reminders, payout notices, round opened, circle started.
/// </summary>
public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }

    /// <summary>
    /// Optional link to a specific circle (e.g., "Your round 3 payout").
    /// </summary>
    public int? RelatedCircleId { get; set; }

    /// <summary>
    /// Optional link to a specific round.
    /// </summary>
    public int? RelatedRoundId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
