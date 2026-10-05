using Microsoft.EntityFrameworkCore;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Domain.Enums;
using TmsApi.Infrastructure.Data;

namespace TmsApi.Infrastructure.Services;

public class CircleService : ICircleService
{
    private readonly AppDbContext _db;
    public CircleService(AppDbContext db) { _db = db; }

    public async Task<CircleDetailDto> CreateCircleAsync(CreateCircleDto dto, int organizerId)
    {
        var circle = new Circle { Name = dto.Name, Contribution = dto.Contribution, MeetingLabel = dto.MeetingLabel, OrganizerId = organizerId, CreatedAt = DateTime.UtcNow };
        _db.Circles.Add(circle);
        await _db.SaveChangesAsync();
        _db.CircleMembers.Add(new CircleMember { CircleId = circle.Id, UserId = organizerId, JoinedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();
        return await BuildDetailDto(circle.Id);
    }

    public async Task<List<CircleSummaryDto>> GetMyCirclesAsync(int userId)
    {
        var circles = await _db.CircleMembers.Where(cm => cm.UserId == userId).Select(cm => cm.Circle!).Include(c => c.Organizer).Include(c => c.Members).Include(c => c.Rounds).ToListAsync();
        return circles.Select(MapToSummary).ToList();
    }

    public async Task<CircleDetailDto?> GetCircleByIdAsync(int circleId, int userId)
    {
        if (!await _db.CircleMembers.AnyAsync(cm => cm.CircleId == circleId && cm.UserId == userId)) return null;
        return await BuildDetailDto(circleId);
    }

    public async Task<MemberDto> AddMemberAsync(int circleId, AddMemberDto dto, int organizerId)
    {
        var circle = await GetCircleOrThrow(circleId);
        EnsureOrganizer(circle, organizerId); EnsureForming(circle);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber)
            ?? throw new InvalidOperationException($"No registered user found with phone '{dto.PhoneNumber}'.");
        if (await _db.CircleMembers.AnyAsync(cm => cm.CircleId == circleId && cm.UserId == user.Id))
            throw new InvalidOperationException("This user is already a member.");
        var member = new CircleMember { CircleId = circleId, UserId = user.Id, JoinedAt = DateTime.UtcNow };
        _db.CircleMembers.Add(member);
        await _db.SaveChangesAsync();
        return new MemberDto(user.Id, $"{user.FirstName} {user.LastName}", user.PhoneNumber, user.ProfilePictureUrl, 0, false, member.JoinedAt);
    }

    public async Task RemoveMemberAsync(int circleId, int userId, int organizerId)
    {
        var circle = await GetCircleOrThrow(circleId);
        EnsureOrganizer(circle, organizerId); EnsureForming(circle);
        if (userId == organizerId) throw new InvalidOperationException("The organizer cannot be removed.");
        var member = await _db.CircleMembers.FirstOrDefaultAsync(cm => cm.CircleId == circleId && cm.UserId == userId)
            ?? throw new InvalidOperationException("Member not found.");
        _db.CircleMembers.Remove(member);
        await _db.SaveChangesAsync();
    }

    public async Task<CircleDetailDto> StartCircleAsync(int circleId, int organizerId)
    {
        var circle = await GetCircleOrThrow(circleId);
        EnsureOrganizer(circle, organizerId); EnsureForming(circle);
        var members = await _db.CircleMembers.Where(cm => cm.CircleId == circleId).OrderBy(cm => cm.JoinedAt).ToListAsync();
        if (members.Count < 2) throw new InvalidOperationException("Need at least 2 members to start.");
        for (var i = 0; i < members.Count; i++) members[i].PayoutOrder = i + 1;
        for (var i = 0; i < members.Count; i++)
            _db.Rounds.Add(new Round { CircleId = circleId, RoundNumber = i + 1, Status = i == 0 ? RoundStatus.Open : RoundStatus.Pending, OpenedAt = i == 0 ? DateTime.UtcNow : DateTime.MinValue });
        await _db.SaveChangesAsync();
        var firstRound = await _db.Rounds.FirstAsync(r => r.CircleId == circleId && r.Status == RoundStatus.Open);
        await CreatePaymentRows(firstRound.Id, circleId);
        circle.Status = CircleStatus.Active; circle.StartedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await BuildDetailDto(circleId);
    }

    internal async Task CreatePaymentRows(int roundId, int circleId)
    {
        var ids = await _db.CircleMembers.Where(cm => cm.CircleId == circleId).Select(cm => cm.UserId).ToListAsync();
        _db.Payments.AddRange(ids.Select(uid => new Payment { RoundId = roundId, UserId = uid }));
        await _db.SaveChangesAsync();
    }

    private async Task<Circle> GetCircleOrThrow(int id) => await _db.Circles.FindAsync(id) ?? throw new KeyNotFoundException("Circle not found.");
    private static void EnsureOrganizer(Circle c, int uid) { if (c.OrganizerId != uid) throw new UnauthorizedAccessException("Only the organizer can perform this action."); }
    private static void EnsureForming(Circle c) { if (c.Status != CircleStatus.Forming) throw new InvalidOperationException("Only allowed while the circle is forming."); }

    private async Task<CircleDetailDto> BuildDetailDto(int circleId)
    {
        var c = await _db.Circles.Include(x => x.Organizer).Include(x => x.Members).ThenInclude(m => m.User).Include(x => x.Rounds).FirstAsync(x => x.Id == circleId);
        var currentRound = c.Rounds.Where(r => r.Status == RoundStatus.Open).MinBy(r => r.RoundNumber);
        var members = c.Members.OrderBy(m => m.PayoutOrder).Select(m => new MemberDto(m.UserId, $"{m.User!.FirstName} {m.User!.LastName}", m.User!.PhoneNumber, m.User!.ProfilePictureUrl, m.PayoutOrder, m.HasReceived, m.JoinedAt)).ToList();
        return new CircleDetailDto(c.Id, c.Name, c.Contribution, c.MeetingLabel, c.Status, $"{c.Organizer!.FirstName} {c.Organizer!.LastName}", currentRound?.RoundNumber, c.Members.Count, members);
    }

    private static CircleSummaryDto MapToSummary(Circle c)
    {
        var round = c.Rounds.Where(r => r.Status == RoundStatus.Open).MinBy(r => r.RoundNumber)?.RoundNumber ?? 0;
        return new CircleSummaryDto(c.Id, c.Name, c.Contribution, c.MeetingLabel, c.Status, c.Members.Count, round, c.Organizer is not null ? $"{c.Organizer.FirstName} {c.Organizer.LastName}" : "");
    }
}
