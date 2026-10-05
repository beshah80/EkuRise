namespace EkubApi.Entities;

/// <summary>
/// Payment record for one member in one round. Created when the round is opened.
/// HasPaid tracks whether the organizer marked this member as paid.
/// LateFine is reserved for extra credit (a fixed late fine on the payment row).
/// </summary>
public class Payment
{
    public int Id { get; set; }
    public int RoundId { get; set; }
    public Round? Round { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public bool HasPaid { get; set; }

    public DateTime? PaidAt { get; set; }

    /// <summary>
    /// Fixed late fine in Birr. Zero by default. Extra credit feature.
    /// </summary>
    public decimal LateFine { get; set; } = 0m;
}
