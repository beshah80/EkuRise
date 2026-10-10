using EkubApi.Data;
using EkubApi.DTOs;
using EkubApi.Entities;
using EkubApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace EkubApi.Services;

public class RoundService : IRoundService
{
    private readonly EkubDbContext _db;
    private readonly INotificationService _notifications;

    public RoundService(EkubDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<List<RoundSummaryDto>> GetRoundsAsync(int circleId, int userId, int? roundNumber, RoundStatus? status)
    {
        // Only members of the circle may list its rounds
        var isMember = await _db.CircleMembers
            .AnyAsync(cm => cm.CircleId == circleId && cm.UserId == userId);
        if (!isMember) return [];

        var query = _db.Rounds
            .Where(r => r.CircleId == circleId)
            .Include(r => r.Circle)
            .Include(r => r.Receiver)
            .Include(r => r.Payments)
            .AsQueryable();
        if (roundNumber.HasValue)
        {
            query = query.Where(r => r.RoundNumber == roundNumber.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var rounds = await query.OrderBy(r => r.RoundNumber).ToListAsync();

        // Load payout order → member name map for this circle once
        var membersByOrder = await _db.CircleMembers
            .Where(cm => cm.CircleId == circleId)
            .Include(cm => cm.User)
            .ToDictionaryAsync(cm => cm.PayoutOrder, cm => $"{cm.User!.FirstName} {cm.User!.LastName}");

        return rounds.Select(r => MapToSummary(r, membersByOrder)).ToList();
    }

    public async Task<RoundDetailDto?> GetCurrentRoundAsync(int circleId, int userId)
    {
        // Ensure the user is a member
        var isMember = await _db.CircleMembers
            .AnyAsync(cm => cm.CircleId == circleId && cm.UserId == userId);
        if (!isMember) return null;

        var round = await _db.Rounds
            .Where(r => r.CircleId == circleId && r.Status == RoundStatus.Open)
            .OrderBy(r => r.RoundNumber)
            .FirstOrDefaultAsync();

        if (round is null) return null;

        return await BuildRoundDetailDto(round.Id);
    }

    public async Task<RoundDetailDto?> GetRoundByIdAsync(int circleId, int roundId, int userId)
    {
        var isMember = await _db.CircleMembers
            .AnyAsync(cm => cm.CircleId == circleId && cm.UserId == userId);
        if (!isMember) return null;

        var round = await _db.Rounds
            .FirstOrDefaultAsync(r => r.CircleId == circleId && r.Id == roundId);
        if (round is null) return null;

        return await BuildRoundDetailDto(roundId);
    }

    /// <summary>
    /// Organizer marks a member as paid or unpaid for a round.
    /// Only allowed on Open rounds.
    /// </summary>
    public async Task<RoundDetailDto> MarkPaymentAsync(int circleId, int roundId, MarkPaymentDto dto, int organizerId)
    {
        var circle = await GetCircleOrThrowAsync(circleId);
        EnsureOrganizer(circle, organizerId);

        var round = await GetRoundOrThrowAsync(circleId, roundId);

        if (round.Status != RoundStatus.Open)
        {
            throw new InvalidOperationException("Payments can only be marked for an open round.");
        }

        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.RoundId == roundId && p.UserId == dto.UserId)
            ?? throw new KeyNotFoundException("Payment record not found for this member in this round.");

        payment.HasPaid = dto.HasPaid;
        payment.PaidAt = dto.HasPaid ? DateTime.UtcNow : null;
        payment.LateFine = dto.LateFine ?? payment.LateFine;

        await _db.SaveChangesAsync();

        return await BuildRoundDetailDto(roundId);
    }

    /// <summary>
    /// Pay out the pot for a round. Enforces all Ekub rules:
    /// - Every member must be marked paid
    /// - The receiver is the next name in the fixed payout order (not typed)
    /// - A member can receive at most once
    /// - Can't pay out a round that's already paid out
    /// </summary>
    public async Task<PayoutResultDto> PayOutAsync(int circleId, int roundId, int organizerId)
    {
        var circle = await GetCircleOrThrowAsync(circleId);
        EnsureOrganizer(circle, organizerId);

        var round = await GetRoundOrThrowAsync(circleId, roundId);

        // Rule: can't pay out a round that's already paid out
        if (round.Status == RoundStatus.PaidOut)
        {
            throw new InvalidOperationException("This round has already been paid out.");
        }

        if (round.Status != RoundStatus.Open)
        {
            throw new InvalidOperationException("Can only pay out an open round.");
        }

        // Rule: payout is allowed only when every member is marked paid
        var payments = await _db.Payments
            .Where(p => p.RoundId == roundId)
            .ToListAsync();

        var unpaidMembers = payments.Where(p => !p.HasPaid).ToList();
        if (unpaidMembers.Count > 0)
        {
            var names = await GetMemberNamesAsync(circleId, unpaidMembers.Select(p => p.UserId));
            throw new InvalidOperationException(
                $"Cannot pay out: {unpaidMembers.Count} member(s) have not paid: {string.Join(", ", names)}.");
        }

        // Rule: receiver is randomly drawn from members who haven't received yet
        var eligible = await _db.CircleMembers
            .Include(cm => cm.User)
            .Where(cm => cm.CircleId == circleId && !cm.HasReceived)
            .ToListAsync();

        if (eligible.Count == 0)
            throw new InvalidOperationException("No eligible members left to receive the pot.");

        var receiver = eligible[new Random().Next(eligible.Count)];

        // Calculate pot
        var pot = payments.Count * circle.Contribution;

        // Execute payout
        round.Status = RoundStatus.PaidOut;
        round.ReceiverId = receiver.UserId;
        round.PaidOutAt = DateTime.UtcNow;

        receiver.HasReceived = true;

        await _db.SaveChangesAsync();

        // Check if all members have received → circle completed
        var allReceived = await _db.CircleMembers
            .Where(cm => cm.CircleId == circleId)
            .AllAsync(cm => cm.HasReceived);

        if (allReceived)
        {
            circle.Status = CircleStatus.Completed;
            circle.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        var receiverUser = await _db.Users.FindAsync(receiver.UserId);
        var receiverFullName = $"{receiverUser!.FirstName} {receiverUser!.LastName}";

        // Notify the winner
        await _notifications.CreateAsync(
            receiver.UserId,
            NotificationType.PayoutNotification,
            "🎉 You won the pot!",
            $"Congratulations! You received {pot:N0} ETB from Round {round.RoundNumber} of your circle.",
            circleId,
            round.Id
        );

        // Notify all other members who the winner is
        var allMemberIds = await _db.CircleMembers
            .Where(cm => cm.CircleId == circleId && cm.UserId != receiver.UserId)
            .Select(cm => cm.UserId)
            .ToListAsync();
        foreach (var memberId in allMemberIds)
        {
            await _notifications.CreateAsync(
                memberId,
                NotificationType.PayoutNotification,
                $"Round {round.RoundNumber} complete",
                $"{receiverFullName} received {pot:N0} ETB. Next round coming up!",
                circleId,
                round.Id
            );
        }

        return new PayoutResultDto(
            round.Id,
            round.RoundNumber,
            receiver.UserId,
            receiverFullName,
            pot,
            round.PaidOutAt!.Value
        );
    }

    /// <summary>
    /// Open the next pending round. Only allowed after the current round is paid out.
    /// </summary>
    public async Task<RoundDetailDto> OpenNextRoundAsync(int circleId, int organizerId)
    {
        var circle = await GetCircleOrThrowAsync(circleId);
        EnsureOrganizer(circle, organizerId);

        if (circle.Status == CircleStatus.Completed)
        {
            throw new InvalidOperationException("This circle is already completed.");
        }

        // Check there is no currently open round
        var openRound = await _db.Rounds
            .Where(r => r.CircleId == circleId && r.Status == RoundStatus.Open)
            .FirstOrDefaultAsync();

        if (openRound is not null)
        {
            throw new InvalidOperationException("There is already an open round. Pay it out before opening the next one.");
        }

        // Find the next pending round
        var nextRound = await _db.Rounds
            .Where(r => r.CircleId == circleId && r.Status == RoundStatus.Pending)
            .OrderBy(r => r.RoundNumber)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No more rounds to open. The circle may be completed.");

        nextRound.Status = RoundStatus.Open;
        nextRound.OpenedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Create payment rows for the newly opened round
        await CreatePaymentRowsForRoundAsync(nextRound.Id, circleId);

        // Notify all members that a new round is open
        var memberIds = await _db.CircleMembers
            .Where(cm => cm.CircleId == circleId)
            .Select(cm => cm.UserId)
            .ToListAsync();
        var circleName = circle.Name;
        foreach (var memberId in memberIds)
        {
            await _notifications.CreateAsync(
                memberId,
                NotificationType.RoundOpened,
                $"Round {nextRound.RoundNumber} is open",
                $"Time to pay your contribution for Round {nextRound.RoundNumber} of '{circleName}'.",
                circleId,
                nextRound.Id
            );
        }

        return await BuildRoundDetailDto(nextRound.Id);
    }

    // --- Helpers ---

    private async Task<Circle> GetCircleOrThrowAsync(int circleId)
    {
        return await _db.Circles.FindAsync(circleId)
            ?? throw new KeyNotFoundException("Circle not found.");
    }

    private async Task<Round> GetRoundOrThrowAsync(int circleId, int roundId)
    {
        return await _db.Rounds
            .FirstOrDefaultAsync(r => r.CircleId == circleId && r.Id == roundId)
            ?? throw new KeyNotFoundException("Round not found in this circle.");
    }

    private static void EnsureOrganizer(Circle circle, int userId)
    {
        if (circle.OrganizerId != userId)
        {
            throw new UnauthorizedAccessException("Only the organizer can perform this action.");
        }
    }

    private async Task<List<string>> GetMemberNamesAsync(int circleId, IEnumerable<int> userIds)
    {
        var idList = userIds.ToList();
        var users = await _db.Users
            .Where(u => idList.Contains(u.Id))
            .Select(u => $"{u.FirstName} {u.LastName}")
            .ToListAsync();
        return users;
    }

    private async Task CreatePaymentRowsForRoundAsync(int roundId, int circleId)
    {
        var memberIds = await _db.CircleMembers
            .Where(cm => cm.CircleId == circleId)
            .Select(cm => cm.UserId)
            .ToListAsync();

        var payments = memberIds.Select(userId => new Payment
        {
            RoundId = roundId,
            UserId = userId,
            HasPaid = false,
            LateFine = 0m
        }).ToList();

        _db.Payments.AddRange(payments);
        await _db.SaveChangesAsync();
    }

    private async Task<RoundDetailDto> BuildRoundDetailDto(int roundId)
    {
        var round = await _db.Rounds
            .Include(r => r.Receiver)
            .Include(r => r.Payments).ThenInclude(p => p.User)
            .FirstAsync(r => r.Id == roundId);

        var circle = await _db.Circles
            .Include(c => c.Rounds)
            .FirstAsync(c => c.Id == round.CircleId);

        var totalRounds = circle.Rounds.Count;

        var paymentDtos = round.Payments
            .OrderBy(p => p.User!.FirstName)
            .Select(p => new PaymentDto(
                p.UserId,
                $"{p.User!.FirstName} {p.User!.LastName}",
                p.User!.ProfilePictureUrl,
                p.HasPaid,
                p.PaidAt,
                p.LateFine
            ))
            .ToList();

        var paidCount = round.Payments.Count(p => p.HasPaid);
        var pot = paidCount * circle.Contribution;

        // nextReceiverName = actual winner, only set after PaidOut
        var nextReceiverName = round.Status == RoundStatus.PaidOut && round.Receiver is not null
            ? $"{round.Receiver.FirstName} {round.Receiver.LastName}"
            : null;

        return new RoundDetailDto(
            round.Id,
            round.RoundNumber,
            totalRounds,
            round.Status,
            circle.Contribution,
            pot,
            paidCount,
            round.Payments.Count,
            round.ReceiverId,
            nextReceiverName,
            null, // no scheduled receiver — winner drawn live at payout
            round.OpenedAt,
            round.PaidOutAt,
            paymentDtos
        );
    }

    private static RoundSummaryDto MapToSummary(Round r, Dictionary<int, string> membersByOrder)
    {
        var paidCount = r.Payments.Count(p => p.HasPaid);
        var circle = r.Circle;
        var pot = circle is not null ? paidCount * circle.Contribution : 0m;

        // Only show receiver name after payout
        var receiverName = r.Receiver is not null ? $"{r.Receiver.FirstName} {r.Receiver.LastName}" : null;

        return new RoundSummaryDto(
            r.Id,
            r.RoundNumber,
            r.Status,
            pot,
            paidCount,
            r.Payments.Count,
            r.ReceiverId,
            receiverName,
            null, // no scheduled receiver
            r.OpenedAt,
            r.PaidOutAt
        );
    }
}
