using EkubApi.Enums;

namespace EkubApi.Entities;

/// <summary>
/// A registered user. Phone number is the primary identifier (not email).
/// Users verify their phone via an OTP code before they can use the app.
/// The organizer is also a member and participates like everyone else.
/// </summary>
public class User
{
    public int Id { get; set; }

    /// <summary>
    /// Primary identifier. Ethiopian phone number format (e.g., 09xxxxxxxx).
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Optional email — most users in Ethiopia don't use email.
    /// </summary>
    public string? Email { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string JobType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool IsPinEnabled { get; set; }
    public string? PinHash { get; set; }
    public Language PreferredLanguage { get; set; } = Language.English;

    /// <summary>
    /// Admin users can create and manage the public Ekub catalog (categories, sub-categories).
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// Unique referral code auto-generated at registration (e.g., "EK-AB12CD").
    /// </summary>
    public string ReferralCode { get; set; } = string.Empty;

    /// <summary>
    /// The user who referred this user. Nullable.
    /// </summary>
    public int? ReferredById { get; set; }
    public User? ReferredBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<CircleMember> CircleMemberships { get; set; } = [];
    public ICollection<Circle> OrganizedCircles { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<Feedback> Feedbacks { get; set; } = [];
}
