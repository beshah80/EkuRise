using EkubApi.Enums;

namespace EkubApi.Entities;

/// <summary>
/// A specific Ekub plan under a category, e.g., "Daily 300 ETB" under "Driver Ekub".
/// Shows: daily contribution, total rounds (= member count), total pot amount,
/// start date, and terms & conditions. Users join through the join flow.
/// When started by admin, a Circle is auto-created from the joined members.
/// </summary>
public class EkubSubCategory
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public EkubCategory? Category { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Daily contribution per member in Birr, e.g., 300.
    /// </summary>
    public decimal DailyContribution { get; set; }

    /// <summary>
    /// Number of rounds (= number of members). Each round one person takes the pot.
    /// E.g., 105 rounds means 105 members.
    /// </summary>
    public int TotalRounds { get; set; }

    /// <summary>
    /// The pot a winner receives each round = DailyContribution * TotalRounds.
    /// E.g., 300 * 105 = 31,500 Birr. Stored for display convenience.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// When this Ekub is scheduled to start.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Terms and conditions text shown before a user joins. Must be agreed to.
    /// </summary>
    public string TermsAndConditions { get; set; } = string.Empty;

    /// <summary>
    /// Maximum members allowed (should equal TotalRounds for a proper Ekub).
    /// </summary>
    public int MaxMembers { get; set; }

    /// <summary>
    /// How many users have joined so far.
    /// </summary>
    public int CurrentMemberCount { get; set; }

    public EkubSubCategoryStatus Status { get; set; } = EkubSubCategoryStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The admin who created this sub-category. Becomes the organizer when the Circle is created.
    /// </summary>
    public int CreatedByAdminId { get; set; }
    public User? CreatedByAdmin { get; set; }

    /// <summary>
    /// When the admin starts this Ekub, a Circle is auto-created. This links to it.
    /// </summary>
    public int? CircleId { get; set; }
    public Circle? Circle { get; set; }

    // Navigation
    public ICollection<EkubSubscription> Subscriptions { get; set; } = [];
}
