using TmsApi.Domain.Enums;

namespace TmsApi.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public int? RelatedCircleId { get; set; }
    public int? RelatedRoundId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
