using EkubApi.Enums;

namespace EkubApi.Entities;

/// <summary>
/// One round of the Ekub circle. There is one round per member.
/// The receiver is determined by the fixed payout order, not by a typed name.
/// </summary>
public class Round
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public Circle? Circle { get; set; }

    /// <summary>
    /// 1-based round number. Round N pays out to the member with PayoutOrder == N.
    /// </summary>
    public int RoundNumber { get; set; }

    public RoundStatus Status { get; set; } = RoundStatus.Pending;

    /// <summary>
    /// The user who receives the pot for this round. Null until paid out.
    /// </summary>
    public int? ReceiverId { get; set; }
    public User? Receiver { get; set; }

    public DateTime OpenedAt { get; set; }
    public DateTime? PaidOutAt { get; set; }

    // Navigation
    public ICollection<Payment> Payments { get; set; } = [];
}
