namespace TmsApi.Domain.Entities;

public class CircleMember
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public Circle? Circle { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int PayoutOrder { get; set; }
    public bool HasReceived { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
