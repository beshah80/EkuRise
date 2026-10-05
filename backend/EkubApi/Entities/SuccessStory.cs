namespace EkubApi.Entities;

/// <summary>
/// Testimonial from an Ekub participant, shown under Account > Success Stories.
/// Must be approved by an admin before it's visible.
/// </summary>
public class SuccessStory
{
    public int Id { get; set; }

    /// <summary>
    /// Nullable — the author may have deleted their account.
    /// </summary>
    public int? UserId { get; set; }
    public User? User { get; set; }

    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 1-5 star rating.
    /// </summary>
    public int Rating { get; set; } = 5;

    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
