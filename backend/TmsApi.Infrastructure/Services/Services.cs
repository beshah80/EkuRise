using Microsoft.EntityFrameworkCore;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Domain.Enums;
using TmsApi.Infrastructure.Data;

namespace TmsApi.Infrastructure.Services;

public class RoundService : IRoundService
{
    private readonly AppDbContext _db;
    public RoundService(AppDbContext db) { _db = db; }

    public async Task<List<RoundSummaryDto>> GetRoundsAsync(int circleId, int? roundNumber, RoundStatus? status)
    {
        var q = _db.Rounds.Where(r => r.CircleId == circleId).Include(r => r.Circle).Include(r => r.Receiver).Include(r => r.Payments).AsQueryable();
        if (roundNumber.HasValue) q = q.Where(r => r.RoundNumber == roundNumber.Value);
        if (status.HasValue) q = q.Where(r => r.Status == status.Value);
        return (await q.OrderBy(r => r.RoundNumber).ToListAsync()).Select(MapToSummary).ToList();
    }

    public async Task<RoundDetailDto?> GetCurrentRoundAsync(int circleId, int userId)
    {
        if (!await _db.CircleMembers.AnyAsync(cm => cm.CircleId == circleId && cm.UserId == userId)) return null;
        var round = await _db.Rounds.Where(r => r.CircleId == circleId && r.Status == RoundStatus.Open).OrderBy(r => r.RoundNumber).FirstOrDefaultAsync();
        return round is null ? null : await BuildDetailDto(round.Id);
    }

    public async Task<RoundDetailDto?> GetRoundByIdAsync(int circleId, int roundId, int userId)
    {
        if (!await _db.CircleMembers.AnyAsync(cm => cm.CircleId == circleId && cm.UserId == userId)) return null;
        var round = await _db.Rounds.FirstOrDefaultAsync(r => r.CircleId == circleId && r.Id == roundId);
        return round is null ? null : await BuildDetailDto(roundId);
    }

    public async Task<RoundDetailDto> MarkPaymentAsync(int circleId, int roundId, MarkPaymentDto dto, int organizerId)
    {
        var circle = await GetCircleOrThrow(circleId);
        EnsureOrganizer(circle, organizerId);
        var round = await GetRoundOrThrow(circleId, roundId);
        if (round.Status != RoundStatus.Open) throw new InvalidOperationException("Payments can only be marked for an open round.");
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.RoundId == roundId && p.UserId == dto.UserId)
            ?? throw new KeyNotFoundException("Payment record not found.");
        payment.HasPaid = dto.HasPaid; payment.PaidAt = dto.HasPaid ? DateTime.UtcNow : null;
        if (dto.LateFine.HasValue) payment.LateFine = dto.LateFine.Value;
        await _db.SaveChangesAsync();
        return await BuildDetailDto(roundId);
    }

    public async Task<PayoutResultDto> PayOutAsync(int circleId, int roundId, int organizerId)
    {
        var circle = await GetCircleOrThrow(circleId);
        EnsureOrganizer(circle, organizerId);
        var round = await GetRoundOrThrow(circleId, roundId);
        if (round.Status == RoundStatus.PaidOut) throw new InvalidOperationException("This round has already been paid out.");
        if (round.Status != RoundStatus.Open) throw new InvalidOperationException("Can only pay out an open round.");
        var payments = await _db.Payments.Where(p => p.RoundId == roundId).ToListAsync();
        var unpaid = payments.Where(p => !p.HasPaid).ToList();
        if (unpaid.Count > 0)
        {
            var names = await _db.Users.Where(u => unpaid.Select(p => p.UserId).Contains(u.Id)).Select(u => $"{u.FirstName} {u.LastName}").ToListAsync();
            throw new InvalidOperationException($"Cannot pay out: {string.Join(", ", names)} have not paid.");
        }
        var receiver = await _db.CircleMembers.Include(cm => cm.User).FirstOrDefaultAsync(cm => cm.CircleId == circleId && cm.PayoutOrder == round.RoundNumber)
            ?? throw new InvalidOperationException("No member found for this round's payout position.");
        if (receiver.HasReceived) throw new InvalidOperationException($"{receiver.User?.FirstName} has already received the pot.");
        var pot = payments.Count * circle.Contribution;
        round.Status = RoundStatus.PaidOut; round.ReceiverId = receiver.UserId; round.PaidOutAt = DateTime.UtcNow;
        receiver.HasReceived = true;
        await _db.SaveChangesAsync();
        if (await _db.CircleMembers.Where(cm => cm.CircleId == circleId).AllAsync(cm => cm.HasReceived))
        {
            circle.Status = CircleStatus.Completed; circle.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        var receiverUser = await _db.Users.FindAsync(receiver.UserId);
        return new PayoutResultDto(round.Id, round.RoundNumber, receiver.UserId, $"{receiverUser!.FirstName} {receiverUser!.LastName}", pot, round.PaidOutAt!.Value);
    }

    public async Task<RoundDetailDto> OpenNextRoundAsync(int circleId, int organizerId)
    {
        var circle = await GetCircleOrThrow(circleId);
        EnsureOrganizer(circle, organizerId);
        if (circle.Status == CircleStatus.Completed) throw new InvalidOperationException("This circle is already completed.");
        if (await _db.Rounds.AnyAsync(r => r.CircleId == circleId && r.Status == RoundStatus.Open))
            throw new InvalidOperationException("There is already an open round.");
        var next = await _db.Rounds.Where(r => r.CircleId == circleId && r.Status == RoundStatus.Pending).OrderBy(r => r.RoundNumber).FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No more rounds to open.");
        next.Status = RoundStatus.Open; next.OpenedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        var ids = await _db.CircleMembers.Where(cm => cm.CircleId == circleId).Select(cm => cm.UserId).ToListAsync();
        _db.Payments.AddRange(ids.Select(uid => new Payment { RoundId = next.Id, UserId = uid }));
        await _db.SaveChangesAsync();
        return await BuildDetailDto(next.Id);
    }

    private async Task<Circle> GetCircleOrThrow(int id) => await _db.Circles.FindAsync(id) ?? throw new KeyNotFoundException("Circle not found.");
    private async Task<Round> GetRoundOrThrow(int circleId, int roundId) => await _db.Rounds.FirstOrDefaultAsync(r => r.CircleId == circleId && r.Id == roundId) ?? throw new KeyNotFoundException("Round not found.");
    private static void EnsureOrganizer(Circle c, int uid) { if (c.OrganizerId != uid) throw new UnauthorizedAccessException("Only the organizer can perform this action."); }

    private async Task<RoundDetailDto> BuildDetailDto(int roundId)
    {
        var r = await _db.Rounds.Include(x => x.Receiver).Include(x => x.Payments).ThenInclude(p => p.User).FirstAsync(x => x.Id == roundId);
        var circle = await _db.Circles.Include(c => c.Rounds).FirstAsync(c => c.Id == r.CircleId);
        var paid = r.Payments.Count(p => p.HasPaid);
        var payments = r.Payments.OrderBy(p => p.User!.FirstName).Select(p => new PaymentDto(p.UserId, $"{p.User!.FirstName} {p.User!.LastName}", p.User!.ProfilePictureUrl, p.HasPaid, p.PaidAt, p.LateFine)).ToList();
        return new RoundDetailDto(r.Id, r.RoundNumber, circle.Rounds.Count, r.Status, circle.Contribution, paid * circle.Contribution, paid, r.Payments.Count, r.ReceiverId, r.Receiver is not null ? $"{r.Receiver.FirstName} {r.Receiver.LastName}" : null, r.OpenedAt, r.PaidOutAt, payments);
    }

    private static RoundSummaryDto MapToSummary(Round r)
    {
        var paid = r.Payments.Count(p => p.HasPaid);
        var pot = r.Circle is not null ? paid * r.Circle.Contribution : 0m;
        return new RoundSummaryDto(r.Id, r.RoundNumber, r.Status, pot, paid, r.Payments.Count, r.ReceiverId, r.Receiver is not null ? $"{r.Receiver.FirstName} {r.Receiver.LastName}" : null, r.OpenedAt, r.PaidOutAt);
    }
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    public NotificationService(AppDbContext db) { _db = db; }

    public async Task<List<NotificationDto>> GetNotificationsAsync(int userId)
        => (await _db.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync()).Select(Map).ToList();

    public async Task MarkAsReadAsync(int userId, int notificationId)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId) ?? throw new KeyNotFoundException("Notification not found.");
        n.IsRead = true; await _db.SaveChangesAsync();
    }

    public async Task MarkAllReadAsync(int userId)
        => await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

    public async Task<int> GetUnreadCountAsync(int userId)
        => await _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task CreateAsync(int userId, NotificationType type, string title, string body, int? circleId = null, int? roundId = null)
    {
        _db.Notifications.Add(new Notification { UserId = userId, Type = type, Title = title, Body = body, RelatedCircleId = circleId, RelatedRoundId = roundId });
        await _db.SaveChangesAsync();
    }

    private static NotificationDto Map(Notification n) => new(n.Id, n.Type, n.Title, n.Body, n.IsRead, n.RelatedCircleId, n.RelatedRoundId, n.CreatedAt);
}

public class FeedbackService : IFeedbackService
{
    private readonly AppDbContext _db;
    public FeedbackService(AppDbContext db) { _db = db; }

    public async Task<FeedbackDto> SubmitFeedbackAsync(int userId, CreateFeedbackDto dto)
    {
        var f = new Feedback { UserId = userId, Subject = dto.Subject, Message = dto.Message };
        _db.Feedbacks.Add(f); await _db.SaveChangesAsync();
        return new FeedbackDto(f.Id, f.Subject, f.Message, f.Status, f.CreatedAt);
    }

    public async Task<List<FeedbackDto>> GetMyFeedbacksAsync(int userId)
        => (await _db.Feedbacks.Where(f => f.UserId == userId).OrderByDescending(f => f.CreatedAt).ToListAsync()).Select(f => new FeedbackDto(f.Id, f.Subject, f.Message, f.Status, f.CreatedAt)).ToList();

    public async Task<List<SuccessStoryDto>> GetApprovedSuccessStoriesAsync()
        => (await _db.SuccessStories.Where(s => s.IsApproved).OrderByDescending(s => s.CreatedAt).ToListAsync()).Select(s => new SuccessStoryDto(s.Id, s.AuthorName, s.Content, s.Rating, s.CreatedAt)).ToList();

    public async Task<SuccessStoryDto> SubmitSuccessStoryAsync(int userId, CreateSuccessStoryDto dto)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("User not found.");
        var s = new SuccessStory { UserId = userId, AuthorName = dto.AuthorName ?? $"{user.FirstName} {user.LastName}", Content = dto.Content, Rating = dto.Rating };
        _db.SuccessStories.Add(s); await _db.SaveChangesAsync();
        return new SuccessStoryDto(s.Id, s.AuthorName, s.Content, s.Rating, s.CreatedAt);
    }
}

public class CatalogService : ICatalogService
{
    private readonly AppDbContext _db;
    public CatalogService(AppDbContext db) { _db = db; }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, int adminId)
    {
        await EnsureAdmin(adminId);
        var c = new EkubCategory { Name = dto.Name, Description = dto.Description, IconUrl = dto.IconUrl };
        _db.EkubCategories.Add(c); await _db.SaveChangesAsync();
        return new CategoryDto(c.Id, c.Name, c.Description, c.IconUrl, 0);
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(int userId)
        => (await _db.EkubCategories.Where(c => c.IsActive).Include(c => c.SubCategories).OrderBy(c => c.Name).ToListAsync())
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.IconUrl, c.SubCategories.Count(s => s.Status != EkubSubCategoryStatus.Completed))).ToList();

    public async Task<SubCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto dto, int adminId)
    {
        await EnsureAdmin(adminId);
        var cat = await _db.EkubCategories.FindAsync(dto.CategoryId) ?? throw new KeyNotFoundException("Category not found.");
        var s = new EkubSubCategory { CategoryId = dto.CategoryId, Name = dto.Name, DailyContribution = dto.DailyContribution, TotalRounds = dto.TotalRounds, TotalAmount = dto.DailyContribution * dto.TotalRounds, StartDate = dto.StartDate, TermsAndConditions = dto.TermsAndConditions, MaxMembers = dto.MaxMembers > 0 ? dto.MaxMembers : dto.TotalRounds, CreatedByAdminId = adminId };
        _db.EkubSubCategories.Add(s); await _db.SaveChangesAsync();
        _db.EkubSubscriptions.Add(new EkubSubscription { UserId = adminId, SubCategoryId = s.Id, AgreedToTerms = true });
        s.CurrentMemberCount = 1; await _db.SaveChangesAsync();
        return MapSub(s, cat.Name, false);
    }

    public async Task StartEkubAsync(int subCategoryId, int adminId)
    {
        await EnsureAdmin(adminId);
        var sub = await _db.EkubSubCategories.Include(s => s.Subscriptions).FirstOrDefaultAsync(s => s.Id == subCategoryId) ?? throw new KeyNotFoundException("Sub-category not found.");
        if (sub.Status != EkubSubCategoryStatus.Open && sub.Status != EkubSubCategoryStatus.Full) throw new InvalidOperationException("Already started or completed.");
        var subs = sub.Subscriptions.OrderBy(s => s.JoinedAt).ToList();
        if (subs.Count < 2) throw new InvalidOperationException("Need at least 2 members.");
        var circle = new Circle { Name = sub.Name, Contribution = sub.DailyContribution, MeetingLabel = "Daily", Status = CircleStatus.Active, OrganizerId = sub.CreatedByAdminId, StartedAt = DateTime.UtcNow };
        _db.Circles.Add(circle); await _db.SaveChangesAsync();
        for (var i = 0; i < subs.Count; i++) _db.CircleMembers.Add(new CircleMember { CircleId = circle.Id, UserId = subs[i].UserId, PayoutOrder = i + 1, JoinedAt = subs[i].JoinedAt });
        await _db.SaveChangesAsync();
        for (var i = 0; i < subs.Count; i++) _db.Rounds.Add(new Round { CircleId = circle.Id, RoundNumber = i + 1, Status = i == 0 ? RoundStatus.Open : RoundStatus.Pending, OpenedAt = i == 0 ? DateTime.UtcNow : DateTime.MinValue });
        await _db.SaveChangesAsync();
        var firstRoundId = await _db.Rounds.Where(r => r.CircleId == circle.Id && r.Status == RoundStatus.Open).Select(r => r.Id).FirstAsync();
        _db.Payments.AddRange(subs.Select(s => new Payment { RoundId = firstRoundId, UserId = s.UserId }));
        sub.CircleId = circle.Id; sub.Status = EkubSubCategoryStatus.Started;
        await _db.SaveChangesAsync();
    }

    public async Task<List<SubCategoryDto>> GetSubCategoriesAsync(int categoryId, int userId)
    {
        var subs = await _db.EkubSubCategories.Where(s => s.CategoryId == categoryId).Include(s => s.Category).OrderByDescending(s => s.CreatedAt).ToListAsync();
        var joined = await _db.EkubSubscriptions.Where(s => s.UserId == userId && subs.Select(x => x.Id).Contains(s.SubCategoryId)).Select(s => s.SubCategoryId).ToHashSetAsync();
        return subs.Select(s => MapSub(s, s.Category?.Name ?? "", joined.Contains(s.Id))).ToList();
    }

    public async Task<SubCategoryDetailDto?> GetSubCategoryByIdAsync(int subCategoryId, int userId)
    {
        var s = await _db.EkubSubCategories.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == subCategoryId);
        if (s is null) return null;
        var joined = await _db.EkubSubscriptions.AnyAsync(x => x.UserId == userId && x.SubCategoryId == subCategoryId);
        return new SubCategoryDetailDto(s.Id, s.CategoryId, s.Category?.Name ?? "", s.Name, s.DailyContribution, s.TotalRounds, s.TotalAmount, s.StartDate, s.TermsAndConditions, s.MaxMembers, s.CurrentMemberCount, s.Status, joined, s.CircleId);
    }

    public async Task<JoinResultDto> JoinSubCategoryAsync(int subCategoryId, bool agreedToTerms, int userId)
    {
        var sub = await _db.EkubSubCategories.FirstOrDefaultAsync(s => s.Id == subCategoryId) ?? throw new KeyNotFoundException("Ekub not found.");
        if (sub.Status != EkubSubCategoryStatus.Open) throw new InvalidOperationException("This Ekub is not open for joining.");
        if (sub.CurrentMemberCount >= sub.MaxMembers) throw new InvalidOperationException("This Ekub is full.");
        if (!agreedToTerms) throw new InvalidOperationException("You must agree to the terms and conditions.");
        if (await _db.EkubSubscriptions.AnyAsync(s => s.UserId == userId && s.SubCategoryId == subCategoryId)) throw new InvalidOperationException("You have already joined this Ekub.");
        var subscription = new EkubSubscription { UserId = userId, SubCategoryId = subCategoryId, AgreedToTerms = true };
        _db.EkubSubscriptions.Add(subscription);
        sub.CurrentMemberCount++;
        if (sub.CurrentMemberCount >= sub.MaxMembers) sub.Status = EkubSubCategoryStatus.Full;
        await _db.SaveChangesAsync();
        return new JoinResultDto(subscription.Id, sub.Id, sub.Name, sub.DailyContribution, sub.TotalAmount, sub.StartDate, "You have successfully joined this Ekub!");
    }

    public async Task<List<MyEkubDto>> GetMyEkubsAsync(int userId)
        => (await _db.EkubSubscriptions.Where(s => s.UserId == userId).Include(s => s.SubCategory).ThenInclude(sc => sc!.Category).OrderByDescending(s => s.JoinedAt).ToListAsync())
            .Select(s => new MyEkubDto(s.Id, s.SubCategory!.Id, s.SubCategory!.Category?.Name ?? "", s.SubCategory!.Name, s.SubCategory!.DailyContribution, s.SubCategory!.TotalAmount, s.SubCategory!.StartDate, s.SubCategory!.Status, s.JoinedAt, s.SubCategory!.CircleId)).ToList();

    private async Task EnsureAdmin(int userId)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("User not found.");
        if (!user.IsAdmin) throw new UnauthorizedAccessException("Only admin users can perform this action.");
    }

    private static SubCategoryDto MapSub(EkubSubCategory s, string catName, bool joined)
        => new(s.Id, s.CategoryId, catName, s.Name, s.DailyContribution, s.TotalRounds, s.TotalAmount, s.StartDate, s.MaxMembers, s.CurrentMemberCount, s.Status, joined);
}
