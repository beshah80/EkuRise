using EkubApi.DTOs;
using EkubApi.Enums;
using EkubApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EkubApi.Controllers;

[ApiController]
[Route("api/circles/{circleId}/rounds")]
public class RoundsController : BaseController
{
    private readonly IRoundService _roundService;

    public RoundsController(IRoundService roundService)
    {
        _roundService = roundService;
    }

    /// <summary>
    /// List all rounds for a circle. Optional filters: by round number or status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<RoundSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<RoundSummaryDto>>> GetRounds(
        int circleId,
        [FromQuery] int? roundNumber,
        [FromQuery] RoundStatus? status)
    {
        var userId = GetUserId();
        var rounds = await _roundService.GetRoundsAsync(circleId, userId, roundNumber, status);
        return Ok(rounds);
    }

    /// <summary>
    /// Get the current (open) round for a circle.
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(RoundDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoundDetailDto>> GetCurrentRound(int circleId)
    {
        var userId = GetUserId();
        var round = await _roundService.GetCurrentRoundAsync(circleId, userId);
        return round is null ? NotFound() : Ok(round);
    }

    /// <summary>
    /// Get detailed info for a specific round: payments, pot, receiver.
    /// </summary>
    [HttpGet("{roundId}")]
    [ProducesResponseType(typeof(RoundDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoundDetailDto>> GetRoundById(int circleId, int roundId)
    {
        var userId = GetUserId();
        var round = await _roundService.GetRoundByIdAsync(circleId, roundId, userId);
        return round is null ? NotFound() : Ok(round);
    }

    /// <summary>
    /// Mark a member as paid or unpaid for this round. Organizer only.
    /// </summary>
    [HttpPut("{roundId}/payments")]
    [ProducesResponseType(typeof(RoundDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoundDetailDto>> MarkPayment(
        int circleId,
        int roundId,
        [FromBody] MarkPaymentDto dto)
    {
        var organizerId = GetUserId();
        var round = await _roundService.MarkPaymentAsync(circleId, roundId, dto, organizerId);
        return Ok(round);
    }

    /// <summary>
    /// Pay out the pot for this round to the rightful member.
    /// Enforces: all paid, fixed payout order, receive once, not already paid out.
    /// Organizer only.
    /// </summary>
    [HttpPost("{roundId}/payout")]
    [ProducesResponseType(typeof(PayoutResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PayoutResultDto>> PayOut(int circleId, int roundId)
    {
        var organizerId = GetUserId();
        var result = await _roundService.PayOutAsync(circleId, roundId, organizerId);
        return Ok(result);
    }

    /// <summary>
    /// Open the next pending round. Only allowed after the current round is paid out.
    /// Organizer only. Route is POST api/circles/{circleId}/rounds/next.
    /// </summary>
    [HttpPost("next")]
    [ProducesResponseType(typeof(RoundDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoundDetailDto>> OpenNext(int circleId)
    {
        var organizerId = GetUserId();
        var round = await _roundService.OpenNextRoundAsync(circleId, organizerId);
        return Ok(round);
    }
}
