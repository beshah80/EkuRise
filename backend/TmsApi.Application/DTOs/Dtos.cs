using System.ComponentModel.DataAnnotations;
using TmsApi.Domain.Enums;

namespace TmsApi.Application.DTOs;

// --- Circle ---
public record CreateCircleDto
{
    [Required] [StringLength(200, MinimumLength = 3)] public string Name { get; init; } = string.Empty;
    [Required] [Range(1, double.MaxValue)] public decimal Contribution { get; init; }
    [Required] [StringLength(50)] public string MeetingLabel { get; init; } = string.Empty;
}

public record AddMemberDto { [Required] [StringLength(20, MinimumLength = 10)] public string PhoneNumber { get; init; } = string.Empty; }
public record MemberDto(int UserId, string FullName, string PhoneNumber, string? ProfilePictureUrl, int PayoutOrder, bool HasReceived, DateTime JoinedAt);
public record CircleSummaryDto(int Id, string Name, decimal Contribution, string MeetingLabel, CircleStatus Status, int MemberCount, int CurrentRoundNumber, string OrganizerName);
public record CircleDetailDto(int Id, string Name, decimal Contribution, string MeetingLabel, CircleStatus Status, string OrganizerName, int? CurrentRoundNumber, int MemberCount, List<MemberDto> Members);

// --- Round ---
public record PaymentDto(int UserId, string MemberName, string? ProfilePictureUrl, bool HasPaid, DateTime? PaidAt, decimal LateFine);
public record RoundDetailDto(int Id, int RoundNumber, int TotalRounds, RoundStatus Status, decimal Contribution, decimal Pot, int PaidCount, int TotalMembers, int? ReceiverId, string? ReceiverName, DateTime OpenedAt, DateTime? PaidOutAt, List<PaymentDto> Payments);
public record RoundSummaryDto(int Id, int RoundNumber, RoundStatus Status, decimal Pot, int PaidCount, int TotalMembers, int? ReceiverId, string? ReceiverName, DateTime OpenedAt, DateTime? PaidOutAt);
public record PayoutResultDto(int RoundId, int RoundNumber, int ReceiverId, string ReceiverName, decimal PotAmount, DateTime PaidOutAt);

public record MarkPaymentDto
{
    [Required] public int UserId { get; init; }
    [Required] public bool HasPaid { get; init; }
    public decimal? LateFine { get; init; }
}

// --- Catalog ---
public record CreateCategoryDto
{
    [Required] [StringLength(100, MinimumLength = 2)] public string Name { get; init; } = string.Empty;
    [StringLength(500)] public string? Description { get; init; }
    public string? IconUrl { get; init; }
}

public record CategoryDto(int Id, string Name, string? Description, string? IconUrl, int SubCategoryCount);

public record CreateSubCategoryDto
{
    [Required] public int CategoryId { get; init; }
    [Required] [StringLength(200, MinimumLength = 3)] public string Name { get; init; } = string.Empty;
    [Required] [Range(1, double.MaxValue)] public decimal DailyContribution { get; init; }
    [Required] [Range(2, int.MaxValue)] public int TotalRounds { get; init; }
    [Required] public DateTime StartDate { get; init; }
    [Required] [StringLength(5000, MinimumLength = 20)] public string TermsAndConditions { get; init; } = string.Empty;
    [Range(2, int.MaxValue)] public int MaxMembers { get; init; }
}

public record SubCategoryDto(int Id, int CategoryId, string CategoryName, string Name, decimal DailyContribution, int TotalRounds, decimal TotalAmount, DateTime StartDate, int MaxMembers, int CurrentMemberCount, EkubSubCategoryStatus Status, bool HasJoined);
public record SubCategoryDetailDto(int Id, int CategoryId, string CategoryName, string Name, decimal DailyContribution, int TotalRounds, decimal TotalAmount, DateTime StartDate, string TermsAndConditions, int MaxMembers, int CurrentMemberCount, EkubSubCategoryStatus Status, bool HasJoined, int? CircleId);
public record JoinResultDto(int SubscriptionId, int SubCategoryId, string SubCategoryName, decimal DailyContribution, decimal TotalAmount, DateTime StartDate, string Message);
public record JoinSubCategoryDto { [Required] public bool AgreedToTerms { get; init; } }
public record MyEkubDto(int SubscriptionId, int SubCategoryId, string CategoryName, string SubCategoryName, decimal DailyContribution, decimal TotalAmount, DateTime StartDate, EkubSubCategoryStatus Status, DateTime JoinedAt, int? CircleId);

// --- Notifications / Feedback ---
public record NotificationDto(int Id, NotificationType Type, string Title, string Body, bool IsRead, int? RelatedCircleId, int? RelatedRoundId, DateTime CreatedAt);

public record CreateFeedbackDto
{
    [Required] [StringLength(200)] public string Subject { get; init; } = string.Empty;
    [Required] [StringLength(2000, MinimumLength = 10)] public string Message { get; init; } = string.Empty;
}

public record FeedbackDto(int Id, string Subject, string Message, FeedbackStatus Status, DateTime CreatedAt);

public record CreateSuccessStoryDto
{
    [Required] [StringLength(2000, MinimumLength = 20)] public string Content { get; init; } = string.Empty;
    [Range(1, 5)] public int Rating { get; init; } = 5;
    [StringLength(150)] public string? AuthorName { get; init; }
}

public record SuccessStoryDto(int Id, string AuthorName, string Content, int Rating, DateTime CreatedAt);
