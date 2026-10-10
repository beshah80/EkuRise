using EkubApi.Enums;

namespace EkubApi.Entities;

public class CircleJoinRequest
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public Circle? Circle { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public CircleJoinRequestStatus Status { get; set; } = CircleJoinRequestStatus.Pending;
    public bool AgreedToTerms { get; set; }
    public string? Message { get; set; }
    /// <summary>Organizer marks this true after confirming the requester has paid the contribution.</summary>
    public bool HasPaid { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}
