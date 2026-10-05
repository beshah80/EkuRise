using System.ComponentModel.DataAnnotations;
using EkubApi.Enums;

namespace EkubApi.DTOs;

// --- Categories ---

/// <summary>
/// Admin creates a main Ekub category (e.g., "Driver Equb", "Trader Equb").
/// </summary>
public record CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; init; }

    public string? IconUrl { get; init; }
}

/// <summary>
/// Category shown on the home page with count of available sub-categories.
/// </summary>
public record CategoryDto(
    int Id,
    string Name,
    string? Description,
    string? IconUrl,
    int SubCategoryCount
);

// --- Sub-Categories ---

/// <summary>
/// Admin creates a sub-category (specific Ekub plan) under a category.
/// </summary>
public record CreateSubCategoryDto
{
    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; init; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 3)]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Daily contribution is required")]
    [Range(1, double.MaxValue, ErrorMessage = "Daily contribution must be greater than 0")]
    public decimal DailyContribution { get; init; }

    [Required(ErrorMessage = "Total rounds is required")]
    [Range(2, int.MaxValue, ErrorMessage = "Must have at least 2 rounds")]
    public int TotalRounds { get; init; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; init; }

    [Required(ErrorMessage = "Terms and conditions are required")]
    [StringLength(5000, MinimumLength = 20, ErrorMessage = "Terms must be at least 20 characters")]
    public string TermsAndConditions { get; init; } = string.Empty;

    [Range(2, int.MaxValue, ErrorMessage = "Max members must be at least 2")]
    public int MaxMembers { get; init; }
}

/// <summary>
/// Sub-category shown in the catalog list (compact).
/// </summary>
public record SubCategoryDto(
    int Id,
    int CategoryId,
    string CategoryName,
    string Name,
    decimal DailyContribution,
    int TotalRounds,
    decimal TotalAmount,
    DateTime StartDate,
    int MaxMembers,
    int CurrentMemberCount,
    EkubSubCategoryStatus Status,
    bool HasJoined
);

/// <summary>
/// Full sub-category detail including terms & conditions (shown when user clicks Join).
/// </summary>
public record SubCategoryDetailDto(
    int Id,
    int CategoryId,
    string CategoryName,
    string Name,
    decimal DailyContribution,
    int TotalRounds,
    decimal TotalAmount,
    DateTime StartDate,
    string TermsAndConditions,
    int MaxMembers,
    int CurrentMemberCount,
    EkubSubCategoryStatus Status,
    bool HasJoined,
    int? CircleId
);

/// <summary>
/// Result after a user joins a sub-category.
/// </summary>
public record JoinResultDto(
    int SubscriptionId,
    int SubCategoryId,
    string SubCategoryName,
    decimal DailyContribution,
    decimal TotalAmount,
    DateTime StartDate,
    string Message
);

/// <summary>
/// Request to join a sub-category.
/// </summary>
public record JoinSubCategoryDto
{
    [Required(ErrorMessage = "You must agree to the terms and conditions")]
    public bool AgreedToTerms { get; init; }
}

/// <summary>
/// The user's joined Ekubs shown under "My Ekubs" or "Ekub History".
/// </summary>
public record MyEkubDto(
    int SubscriptionId,
    int SubCategoryId,
    string CategoryName,
    string SubCategoryName,
    decimal DailyContribution,
    decimal TotalAmount,
    DateTime StartDate,
    EkubSubCategoryStatus Status,
    DateTime JoinedAt,
    int? CircleId
);
