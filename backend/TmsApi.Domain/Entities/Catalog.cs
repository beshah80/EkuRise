using TmsApi.Domain.Enums;

namespace TmsApi.Domain.Entities;

public class EkubCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<EkubSubCategory> SubCategories { get; set; } = [];
}

public class EkubSubCategory
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public EkubCategory? Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DailyContribution { get; set; }
    public int TotalRounds { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime StartDate { get; set; }
    public string TermsAndConditions { get; set; } = string.Empty;
    public int MaxMembers { get; set; }
    public int CurrentMemberCount { get; set; }
    public EkubSubCategoryStatus Status { get; set; } = EkubSubCategoryStatus.Open;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedByAdminId { get; set; }
    public User? CreatedByAdmin { get; set; }
    public int? CircleId { get; set; }
    public Circle? Circle { get; set; }
    public ICollection<EkubSubscription> Subscriptions { get; set; } = [];
}

public class EkubSubscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int SubCategoryId { get; set; }
    public EkubSubCategory? SubCategory { get; set; }
    public bool AgreedToTerms { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
