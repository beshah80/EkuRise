using EkubApi.DTOs;
using EkubApi.Enums;

namespace EkubApi.Services;

public interface IRoundService
{
    Task<List<RoundSummaryDto>> GetRoundsAsync(int circleId, int? roundNumber, RoundStatus? status);
    Task<RoundDetailDto?> GetRoundByIdAsync(int circleId, int roundId, int userId);
    Task<RoundDetailDto?> GetCurrentRoundAsync(int circleId, int userId);
    Task<RoundDetailDto> MarkPaymentAsync(int circleId, int roundId, MarkPaymentDto dto, int organizerId);
    Task<PayoutResultDto> PayOutAsync(int circleId, int roundId, int organizerId);
    Task<RoundDetailDto> OpenNextRoundAsync(int circleId, int organizerId);
}
