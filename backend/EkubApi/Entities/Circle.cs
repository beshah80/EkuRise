using EkubApi.Enums;

namespace EkubApi.Entities;

/// <summary>
/// A rotating Ekub savings circle. The organizer creates it and sets the contribution
/// and meeting label. Members are added while the circle is Forming. When started,
/// the member list is locked, a payout order is fixed, and rounds are created.
/// </summary>
public class Circle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Fixed contribution amount in Birr that every member pays each round.
    /// Stored as decimal for exact financial math.
    /// </summary>
    public decimal Contribution { get; set; }

    /// <summary>
    /// Label for the meeting frequency (e.g., "Weekly", "Monthly").
    /// This is a label only, not a scheduler.
    /// </summary>
    public string MeetingLabel { get; set; } = string.Empty;

    public CircleStatus Status { get; set; } = CircleStatus.Forming;

    /// <summary>
    /// The user who created and manages this circle.
    /// </summary>
    public int OrganizerId { get; set; }
    public User? Organizer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Set when the circle is started (member list locked, rounds created).
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Set when every member has received the pot once.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public ICollection<CircleMember> Members { get; set; } = [];
    public ICollection<Round> Rounds { get; set; } = [];
}
