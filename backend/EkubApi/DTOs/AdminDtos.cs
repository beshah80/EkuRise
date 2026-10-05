using EkubApi.Enums;

namespace EkubApi.DTOs;

public record AdminStatsDto(
    int TotalUsers,
    int TotalCircles,
    int ActiveCircles,
    int FormingCircles,
    int CompletedCircles,
    int TotalCategories,
    int TotalSubCategories,
    int PendingStories,
    int PendingFeedbacks,
    decimal TotalSavingsVolume,
    int PendingSubscriptions = 0
);

public record AdminUserDto(
    int Id,
    string PhoneNumber,
    string? Email,
    string FirstName,
    string LastName,
    string JobType,
    string Location,
    bool IsPhoneVerified,
    bool IsAdmin,
    int JoinedCirclesCount,
    DateTime CreatedAt
);

public record AdminStoryDto(
    int Id,
    int? UserId,
    string AuthorName,
    string? UserPhoneNumber,
    string Content,
    int Rating,
    bool IsApproved,
    DateTime CreatedAt
);

public record AdminFeedbackDto(
    int Id,
    int UserId,
    string UserName,
    string UserPhoneNumber,
    string Subject,
    string Message,
    FeedbackStatus Status,
    DateTime CreatedAt
);

public record UpdateFeedbackStatusDto(
    FeedbackStatus Status
);
