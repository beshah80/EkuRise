using System.ComponentModel.DataAnnotations;
using EkubApi.Enums;

namespace EkubApi.DTOs;

/// <summary>
/// Request to create a new circle. The creating user becomes the organizer.
/// </summary>
public record CreateCircleDto
{
    [Required(ErrorMessage = "Circle name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Circle name must be at least 3 characters")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Contribution amount is required")]
    [Range(1, double.MaxValue, ErrorMessage = "Contribution must be greater than 0")]
    public decimal Contribution { get; init; }

    [Required(ErrorMessage = "Meeting label is required")]
    [StringLength(50, ErrorMessage = "Meeting label must not exceed 50 characters")]
    public string MeetingLabel { get; init; } = string.Empty;
}

/// <summary>
/// Request to add a member to a forming circle by phone number.
/// </summary>
public record AddMemberDto
{
    [Required(ErrorMessage = "Phone number is required")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "Enter a valid phone number")]
    public string PhoneNumber { get; init; } = string.Empty;
}

/// <summary>
/// A member's info within a circle, including payout position and receive status.
/// </summary>
public record MemberDto(
    int UserId,
    string FullName,
    string PhoneNumber,
    string? ProfilePictureUrl,
    int PayoutOrder,
    bool HasReceived,
    DateTime JoinedAt
);

/// <summary>
/// Summary of a circle shown in lists.
/// </summary>
public record CircleSummaryDto(
    int Id,
    string Name,
    decimal Contribution,
    string MeetingLabel,
    CircleStatus Status,
    int MemberCount,
    int CurrentRoundNumber,
    string OrganizerName,
    int OrganizerId
);

/// <summary>
/// Full circle detail shown on the circle home page.
/// </summary>
public record CircleDetailDto(
    int Id,
    string Name,
    decimal Contribution,
    string MeetingLabel,
    CircleStatus Status,
    int OrganizerId,
    string OrganizerName,
    int? CurrentRoundNumber,
    int MemberCount,
    List<MemberDto> Members
);

/// <summary>
/// Past round winner summary used in member home history.
/// </summary>
public record RoundWinnerDto(
    int RoundNumber,
    int ReceiverId,
    string ReceiverName,
    decimal Pot,
    DateTime PaidOutAt
);

/// <summary>
/// Member home screen: everything the calling member needs in one call.
/// - HasPaidCurrentRound: whether they paid the currently open round
/// - HasReceived: whether they have ever received the pot
/// - CurrentPot: pot accumulated so far this round (paid members × contribution)
/// - CurrentRoundNumber: which round is open right now (null if circle not active)
/// - WinnerHistory: every past round winner in order
/// </summary>
public record MemberHomeDto(
    bool HasPaidCurrentRound,
    bool HasReceived,
    decimal CurrentPot,
    int? CurrentRoundNumber,
    int PayoutOrder,
    List<RoundWinnerDto> WinnerHistory
);
