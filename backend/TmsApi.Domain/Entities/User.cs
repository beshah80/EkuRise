using TmsApi.Domain.Enums;

namespace TmsApi.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
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
    public bool IsAdmin { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public int? ReferredById { get; set; }
    public User? ReferredBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<CircleMember> CircleMemberships { get; set; } = [];
    public ICollection<Circle> OrganizedCircles { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<Feedback> Feedbacks { get; set; } = [];
}
