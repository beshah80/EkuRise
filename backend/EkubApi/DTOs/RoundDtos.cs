using System.ComponentModel.DataAnnotations;
using EkubApi.Enums;

namespace EkubApi.DTOs;

/// <summary>
/// One member's payment status in a round.
/// </summary>
public record PaymentDto(
    int UserId,
    string MemberName,
    string? ProfilePictureUrl,
    bool HasPaid,
    DateTime? PaidAt,
    decimal LateFine
);

/// <summary>
/// Round detail with all payment rows and the pot total.
/// </summary>
public record RoundDetailDto(
    int Id,
    int RoundNumber,
    int TotalRounds,
    RoundStatus Status,
    decimal Contribution,
    decimal Pot,
    int PaidCount,
    int TotalMembers,
    int? ReceiverId,
    string? ReceiverName,
    string? NextReceiverName,
    DateTime? OpenedAt,
    DateTime? PaidOutAt,
    List<PaymentDto> Payments
);

/// <summary>
/// Request to mark a member's payment status for the current round.
/// </summary>
public record MarkPaymentDto
{
    [Required(ErrorMessage = "UserId is required")]
    public int UserId { get; init; }

    [Required(ErrorMessage = "HasPaid status is required")]
    public bool HasPaid { get; init; }

    /// <summary>
    /// Optional late fine in Birr. Extra credit feature.
    /// </summary>
    public decimal? LateFine { get; init; }
}

/// <summary>
/// Response after a successful payout.
/// </summary>
public record PayoutResultDto(
    int RoundId,
    int RoundNumber,
    int ReceiverId,
    string ReceiverName,
    decimal PotAmount,
    DateTime PaidOutAt
);

/// <summary>
/// Round summary used in the history list.
/// </summary>
public record RoundSummaryDto(
    int Id,
    int RoundNumber,
    RoundStatus Status,
    decimal Pot,
    int PaidCount,
    int TotalMembers,
    int? ReceiverId,
    string? ReceiverName,
    string? NextReceiverName,
    DateTime? OpenedAt,
    DateTime? PaidOutAt
);
