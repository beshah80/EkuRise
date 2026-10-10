using EkubApi.Data;
using EkubApi.DTOs;
using EkubApi.Entities;
using EkubApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace EkubApi.Services;

public class CircleService : ICircleService
{
    private readonly EkubDbContext _db;
    private readonly INotificationService _notifications;

    public CircleService(EkubDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<CircleDetailDto> CreateCircleAsync(CreateCircleDto dto, int organizerId)
    {
        var circle = new Circle
        {
            Name = dto.Name,
            Contribution = dto.Contribution,
            MeetingLabel = dto.MeetingLabel,
            Status = CircleStatus.Forming,
            OrganizerId = organizerId,
            CategoryId = dto.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Circles.Add(circle);
        await _db.SaveChangesAsync();

        // The organizer is automatically the first member
        var member = new CircleMember
        {
            CircleId = circle.Id,
            UserId = organizerId,
            JoinedAt = DateTime.UtcNow
        };
        _db.CircleMembers.Add(member);
        await _db.SaveChangesAsync();

        return await BuildCircleDetailDto(circle.Id);
    }

    public async Task<List<CircleSummaryDto>> GetMyCirclesAsync(int userId)
    {
        // Load circle IDs the user belongs to first, then load full circle data
        var circleIds = await _db.CircleMembers
            .Where(cm => cm.UserId == userId)
            .Select(cm => cm.CircleId)
            .ToListAsync();

        var circles = await _db.Circles
            .Where(c => circleIds.Contains(c.Id))
            .Include(c => c.Organizer)
            .Include(c => c.Members)
            .Include(c => c.Rounds)
            .ToListAsync();

        return circles.Select(c => MapToSummary(c)).ToList();
    }

    public async Task<CircleDetailDto?> GetCircleByIdAsync(int circleId, int userId)
    {
        // Ensure the requesting user is a member of this circle
        var isMember = await _db.CircleMembers
            .AnyAsync(cm => cm.CircleId == circleId && cm.UserId == userId);
        if (!isMember) return null;

        return await BuildCircleDetailDto(circleId);
    }

    public async Task<MemberDto> AddMemberAsync(int circleId, AddMemberDto dto, int organizerId)
    {
        var circle = await GetCircleOrThrowAsync(circleId);

        EnsureOrganizer(circle, organizerId);
        EnsureForming(circle);

        // Find the user by phone number
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);
        if (user is null)
        {
            throw new InvalidOperationException($"No registered user found with phone '{dto.PhoneNumber}'. They must register first.");
        }

        // Check if already a member
        var alreadyMember = await _db.CircleMembers
            .AnyAsync(cm => cm.CircleId == circleId && cm.UserId == user.Id);
        if (alreadyMember)
        {
            throw new InvalidOperationException("This user is already a member of the circle.");
        }

        var member = new CircleMember
        {
            CircleId = circleId,
            UserId = user.Id,
            JoinedAt = DateTime.UtcNow
        };
        _db.CircleMembers.Add(member);
        await _db.SaveChangesAsync();

        return new MemberDto(user.Id, $"{user.FirstName} {user.LastName}", user.PhoneNumber, user.ProfilePictureUrl, 0, false, member.JoinedAt);
    }

    public async Task RemoveMemberAsync(int circleId, int userId, int organizerId)
    {
        var circle = await GetCircleOrThrowAsync(circleId);

        EnsureOrganizer(circle, organizerId);
        EnsureForming(circle);

        if (userId == organizerId)
        {
            throw new InvalidOperationException("The organizer cannot be removed from the circle.");
        }

        var member = await _db.CircleMembers
            .FirstOrDefaultAsync(cm => cm.CircleId == circleId && cm.UserId == userId)
            ?? throw new InvalidOperationException("Member not found in this circle.");

        _db.CircleMembers.Remove(member);
        await _db.SaveChangesAsync();
    }

    public async Task<CircleDetailDto> StartCircleAsync(int circleId, int organizerId)
    {
        var circle = await GetCircleOrThrowAsync(circleId);

        EnsureOrganizer(circle, organizerId);
        EnsureForming(circle);

        var members = await _db.CircleMembers
            .Where(cm => cm.CircleId == circleId)
            .ToListAsync();

        if (members.Count < 2)
            throw new InvalidOperationException("A circle needs at least 2 members to start.");

        // Assign display order (1-based, by join time) — NOT payout order
        var ordered = members.OrderBy(m => m.JoinedAt).ToList();
        for (var i = 0; i < ordered.Count; i++)
            ordered[i].PayoutOrder = i + 1;

        // Create one round per member, all Pending except first which is Open
        var rounds = new List<Round>();
        for (var i = 0; i < members.Count; i++)
        {
            rounds.Add(new Round
            {
                CircleId = circleId,
                RoundNumber = i + 1,
                Status = i == 0 ? RoundStatus.Open : RoundStatus.Pending,
                OpenedAt = i == 0 ? DateTime.UtcNow : null
            });
        }
        _db.Rounds.AddRange(rounds);
        await _db.SaveChangesAsync();

        // Create payment rows for the first (open) round
        await CreatePaymentRowsForRoundAsync(rounds[0].Id, circleId);

        circle.Status = CircleStatus.Active;
        circle.StartedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Notify all members the circle has started
        foreach (var m in members)
        {
            await _notifications.CreateAsync(
                m.UserId,
                NotificationType.CircleStarted,
                "Your circle has started! 🎉",
                $"'{circle.Name}' is now active. Round 1 is open — time to pay your contribution.",
                circle.Id,
                rounds[0].Id
            );
        }

        return await BuildCircleDetailDto(circleId);
    }

    /// <summary>
    /// Create one Payment row per member for the given round.
    /// </summary>
    internal async Task CreatePaymentRowsForRoundAsync(int roundId, int circleId)
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

    /// <summary>
    /// Returns everything the calling member needs on their home screen in one call:
    /// whether they paid the current round, whether they have received, the current pot,
    /// and the full history of past round winners.
    /// </summary>
    public async Task<MemberHomeDto?> GetMemberHomeAsync(int circleId, int userId)
    {
        // Verify membership
        var membership = await _db.CircleMembers
            .FirstOrDefaultAsync(cm => cm.CircleId == circleId && cm.UserId == userId);
        if (membership is null) return null;

        var circle = await _db.Circles.FindAsync(circleId);
        if (circle is null) return null;

        // Current open round (null if circle not started or completed)
        var currentRound = await _db.Rounds
            .Include(r => r.Payments)
            .FirstOrDefaultAsync(r => r.CircleId == circleId && r.Status == RoundStatus.Open);

        bool hasPaidCurrentRound = false;
        decimal currentPot = 0m;
        int? currentRoundNumber = null;

        if (currentRound is not null)
        {
            currentRoundNumber = currentRound.RoundNumber;
            var myPayment = currentRound.Payments.FirstOrDefault(p => p.UserId == userId);
            hasPaidCurrentRound = myPayment?.HasPaid ?? false;
            var paidCount = currentRound.Payments.Count(p => p.HasPaid);
            currentPot = paidCount * circle.Contribution;
        }

        // Winner history: every paid-out round, in order
        var paidOutRounds = await _db.Rounds
            .Where(r => r.CircleId == circleId && r.Status == RoundStatus.PaidOut && r.ReceiverId != null)
            .Include(r => r.Receiver)
            .Include(r => r.Payments)
            .OrderBy(r => r.RoundNumber)
            .ToListAsync();

        var winnerHistory = paidOutRounds.Select(r =>
        {
            var pot = r.Payments.Count(p => p.HasPaid) * circle.Contribution;
            return new RoundWinnerDto(
                r.RoundNumber,
                r.ReceiverId!.Value,
                r.Receiver is not null ? $"{r.Receiver.FirstName} {r.Receiver.LastName}" : "Unknown",
                pot,
                r.PaidOutAt!.Value
            );
        }).ToList();

        return new MemberHomeDto(
            hasPaidCurrentRound,
            membership.HasReceived,
            currentPot,
            currentRoundNumber,
            membership.PayoutOrder,
            winnerHistory
        );
    }

    // --- Join Request Methods ---

    public async Task<List<PublicCircleSummaryDto>> GetPublicCirclesAsync(int userId, int? categoryId)
    {
        var query = _db.Circles
            .Where(c => c.Status == CircleStatus.Forming)
            .Include(c => c.Organizer)
            .Include(c => c.Category)
            .Include(c => c.Members)
            .Include(c => c.JoinRequests.Where(r => r.UserId == userId))
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId == categoryId.Value);
        }

        var circles = await query.ToListAsync();

        return circles.Select(c => new PublicCircleSummaryDto(
            c.Id,
            c.Name,
            c.Contribution,
            c.MeetingLabel,
            (int)c.Status,
            c.Members.Count,
            c.Organizer is not null ? $"{c.Organizer.FirstName} {c.Organizer.LastName}" : "Unknown",
            c.OrganizerId,
            c.Category?.Name,
            c.JoinRequests.Any(r => r.UserId == userId && r.Status == CircleJoinRequestStatus.Pending),
            c.Members.Any(m => m.UserId == userId)
        )).ToList();
    }

    public async Task<JoinRequestDto> SubmitJoinRequestAsync(int circleId, int userId, SubmitJoinRequestDto dto)
    {
        var circle = await _db.Circles
            .Include(c => c.Members)
            .Include(c => c.JoinRequests.Where(r => r.UserId == userId))
            .FirstOrDefaultAsync(c => c.Id == circleId)
            ?? throw new KeyNotFoundException("Circle not found.");

        if (circle.Status != CircleStatus.Forming)
            throw new InvalidOperationException("You can only request to join a circle that is currently forming.");

        if (circle.Members.Any(m => m.UserId == userId))
            throw new InvalidOperationException("Already a member.");

        if (circle.JoinRequests.Any(r => r.UserId == userId && r.Status == CircleJoinRequestStatus.Pending))
            throw new InvalidOperationException("Join request already pending.");

        var request = new CircleJoinRequest
        {
            CircleId = circleId,
            UserId = userId,
            Status = CircleJoinRequestStatus.Pending,
            AgreedToTerms = dto.AgreedToTerms,
            Message = dto.Message,
            CreatedAt = DateTime.UtcNow
        };

        _db.CircleJoinRequests.Add(request);
        await _db.SaveChangesAsync();

        // Load user for the response
        var user = await _db.Users.FindAsync(userId);

        return new JoinRequestDto(
            request.Id,
            request.CircleId,
            request.UserId,
            user is not null ? $"{user.FirstName} {user.LastName}" : "Unknown",
            user?.PhoneNumber ?? "",
            (int)request.Status,
            request.AgreedToTerms,
            request.HasPaid,
            request.Message,
            request.CreatedAt
        );
    }

    public async Task<List<JoinRequestDto>> GetJoinRequestsAsync(int circleId, int organizerId)
    {
        var circle = await _db.Circles.FindAsync(circleId)
            ?? throw new KeyNotFoundException("Circle not found.");

        if (circle.OrganizerId != organizerId)
            throw new UnauthorizedAccessException("Only the organizer can view join requests.");

        var requests = await _db.CircleJoinRequests
            .Where(r => r.CircleId == circleId && r.Status == CircleJoinRequestStatus.Pending)
            .Include(r => r.User)
            .ToListAsync();

        return requests.Select(r => new JoinRequestDto(
            r.Id,
            r.CircleId,
            r.UserId,
            r.User is not null ? $"{r.User.FirstName} {r.User.LastName}" : "Unknown",
            r.User?.PhoneNumber ?? "",
            (int)r.Status,
            r.AgreedToTerms,
            r.HasPaid,
            r.Message,
            r.CreatedAt
        )).ToList();
    }

    public async Task<JoinRequestDto> ReviewJoinRequestAsync(int circleId, int requestId, int organizerId, ReviewJoinRequestDto dto)
    {
        var request = await _db.CircleJoinRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == requestId)
            ?? throw new KeyNotFoundException("Join request not found.");

        var circle = await _db.Circles.FindAsync(circleId)
            ?? throw new KeyNotFoundException("Circle not found.");

        if (circle.OrganizerId != organizerId)
            throw new UnauthorizedAccessException("Only the organizer can review join requests.");

        if (request.Status != CircleJoinRequestStatus.Pending)
            throw new InvalidOperationException("Already reviewed.");

        if (dto.Approved)
        {
            if (!request.HasPaid)
                throw new InvalidOperationException("Cannot approve: the requester has not paid the contribution yet. Mark as paid first.");

            request.Status = CircleJoinRequestStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow;

            _db.CircleMembers.Add(new CircleMember
            {
                CircleId = circleId,
                UserId = request.UserId,
                PayoutOrder = 0,
                JoinedAt = DateTime.UtcNow
            });
        }
        else
        {
            request.Status = CircleJoinRequestStatus.Rejected;
            request.ReviewedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return new JoinRequestDto(
            request.Id,
            request.CircleId,
            request.UserId,
            request.User is not null ? $"{request.User.FirstName} {request.User.LastName}" : "Unknown",
            request.User?.PhoneNumber ?? "",
            (int)request.Status,
            request.AgreedToTerms,
            request.HasPaid,
            request.Message,
            request.CreatedAt
        );
    }

    public async Task<JoinRequestDto> MarkJoinRequestPaidAsync(int circleId, int requestId, int organizerId, bool hasPaid)
    {
        var circle = await _db.Circles.FindAsync(circleId)
            ?? throw new KeyNotFoundException("Circle not found.");

        if (circle.OrganizerId != organizerId)
            throw new UnauthorizedAccessException("Only the organizer can mark payment.");

        var request = await _db.CircleJoinRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.CircleId == circleId)
            ?? throw new KeyNotFoundException("Join request not found.");

        if (request.Status != CircleJoinRequestStatus.Pending)
            throw new InvalidOperationException("Request is no longer pending.");

        request.HasPaid = hasPaid;
        await _db.SaveChangesAsync();

        return new JoinRequestDto(
            request.Id,
            request.CircleId,
            request.UserId,
            request.User is not null ? $"{request.User.FirstName} {request.User.LastName}" : "Unknown",
            request.User?.PhoneNumber ?? "",
            (int)request.Status,
            request.AgreedToTerms,
            request.HasPaid,
            request.Message,
            request.CreatedAt
        );
    }

    // --- Helpers ---

    private async Task<Circle> GetCircleOrThrowAsync(int circleId)
    {
        return await _db.Circles.FindAsync(circleId)
            ?? throw new KeyNotFoundException("Circle not found.");
    }

    private static void EnsureOrganizer(Circle circle, int userId)
    {
        if (circle.OrganizerId != userId)
        {
            throw new UnauthorizedAccessException("Only the organizer can perform this action.");
        }
    }

    private static void EnsureForming(Circle circle)
    {
        if (circle.Status != CircleStatus.Forming)
        {
            throw new InvalidOperationException("This action is only allowed while the circle is forming.");
        }
    }

    private async Task<CircleDetailDto> BuildCircleDetailDto(int circleId)
    {
        var circle = await _db.Circles
            .Include(c => c.Organizer)
            .Include(c => c.Members).ThenInclude(cm => cm.User)
            .Include(c => c.Rounds)
            .FirstAsync(c => c.Id == circleId);

        var currentRound = circle.Rounds
            .Where(r => r.Status == RoundStatus.Open)
            .MinBy(r => r.RoundNumber);

        var memberDtos = circle.Members
            .OrderBy(cm => cm.PayoutOrder)
            .Select(cm => new MemberDto(
                cm.UserId,
                $"{cm.User!.FirstName} {cm.User!.LastName}",
                cm.User!.PhoneNumber,
                cm.User!.ProfilePictureUrl,
                cm.PayoutOrder,
                cm.HasReceived,
                cm.JoinedAt
            ))
            .ToList();

        return new CircleDetailDto(
            circle.Id,
            circle.Name,
            circle.Contribution,
            circle.MeetingLabel,
            circle.Status,
            circle.OrganizerId,
            $"{circle.Organizer!.FirstName} {circle.Organizer!.LastName}",
            currentRound?.RoundNumber,
            circle.Members.Count,
            memberDtos
        );
    }

    private static CircleSummaryDto MapToSummary(Circle c)
    {
        var currentRoundNumber = c.Rounds
            .Where(r => r.Status == RoundStatus.Open)
            .MinBy(r => r.RoundNumber)?.RoundNumber;

        var organizerName = c.Organizer is not null
            ? $"{c.Organizer.FirstName} {c.Organizer.LastName}"
            : "Unknown";

        return new CircleSummaryDto(
            c.Id,
            c.Name,
            c.Contribution,
            c.MeetingLabel,
            c.Status,
            c.Members.Count,
            currentRoundNumber ?? 0,
            organizerName,
            c.OrganizerId
        );
    }
}
