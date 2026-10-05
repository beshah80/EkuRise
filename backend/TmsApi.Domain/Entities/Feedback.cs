using TmsApi.Domain.Enums;

namespace TmsApi.Domain.Entities;

public class Feedback
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
