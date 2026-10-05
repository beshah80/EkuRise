namespace EkubApi.Entities;

/// <summary>
/// Junction between Circle and User. The PayoutOrder is the fixed position
/// in the payout sequence, set when the circle is started. HasReceived
/// tracks whether this member has taken the pot (at most once).
/// </summary>
public class CircleMember
{
    public int Id { get; set; }
    public int CircleId { get; set; }
    public Circle? Circle { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    /// <summary>
    /// Position in the fixed payout order (1-based). Set at Start.
    /// Round N pays out to the member with PayoutOrder == N.
    /// </summary>
    public int PayoutOrder { get; set; }

    /// <summary>
    /// Whether this member has already received the pot. A member can
    /// receive at most once but continues to pay after receiving.
    /// </summary>
    public bool HasReceived { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
