namespace EkubApi.Entities;

/// <summary>
/// Records that a user joined a public Ekub sub-category and agreed to its terms.
/// When the admin starts the Ekub, all subscribers become members of the auto-created Circle.
/// </summary>
public class EkubSubscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public int SubCategoryId { get; set; }
    public EkubSubCategory? SubCategory { get; set; }

    /// <summary>
    /// Whether the user agreed to the terms and conditions.
    /// </summary>
    public bool AgreedToTerms { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
