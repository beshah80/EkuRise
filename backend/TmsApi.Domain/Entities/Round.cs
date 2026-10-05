using TmsApi.Domain.Enums;

namespace TmsApi.Domain.Entities;

public class Round
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public Circle? Circle { get; set; }
    public int RoundNumber { get; set; }
    public RoundStatus Status { get; set; } = RoundStatus.Pending;
    public int? ReceiverId { get; set; }
    public User? Receiver { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? PaidOutAt { get; set; }
    public ICollection<Payment> Payments { get; set; } = [];
}
