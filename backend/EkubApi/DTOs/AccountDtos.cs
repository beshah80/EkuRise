using System.ComponentModel.DataAnnotations;
using EkubApi.Enums;

namespace EkubApi.DTOs;

// --- Notifications ---

public record NotificationDto(
    int Id,
    NotificationType Type,
    string Title,
    string Body,
    bool IsRead,
    int? RelatedCircleId,
    int? RelatedRoundId,
    DateTime CreatedAt
);

// --- Feedback ---

public record CreateFeedbackDto
{
    [Required(ErrorMessage = "Subject is required")]
    [StringLength(200)]
    public string Subject { get; init; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be at least 10 characters")]
    public string Message { get; init; } = string.Empty;
}

public record FeedbackDto(
    int Id,
    string Subject,
    string Message,
    FeedbackStatus Status,
    DateTime CreatedAt
);

// --- Success Stories ---

public record CreateSuccessStoryDto
{
    [Required(ErrorMessage = "Content is required")]
    [StringLength(2000, MinimumLength = 20, ErrorMessage = "Story must be at least 20 characters")]
    public string Content { get; init; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; init; } = 5;

    [StringLength(150)]
    public string? AuthorName { get; init; }
}

public record SuccessStoryDto(
    int Id,
    string AuthorName,
    string Content,
    int Rating,
    DateTime CreatedAt
);
