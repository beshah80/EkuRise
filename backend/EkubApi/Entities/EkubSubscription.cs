using EkubApi.Enums;

namespace EkubApi.Entities;

/// <summary>
/// Records that a user joined a public Ekub sub-category and agreed to its terms.
/// Requires National ID (FAN) & payment proof approval by Admin before becoming an active member.
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

    /// <summary>
    /// Applicant full name provided during payment submission.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// National ID / Fayda FAN number.
    /// </summary>
    public string? NationalIdFan { get; set; }

    /// <summary>
    /// Screenshot / receipt of the payment (image URL or base64 data).
    /// </summary>
    public string? PaymentProofUrl { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.PendingPayment;

    public string? RejectionReason { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
}
