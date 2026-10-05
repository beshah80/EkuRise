using EkubApi.Data;
using EkubApi.DTOs;
using EkubApi.Entities;
using EkubApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace EkubApi.Services;

public class AdminService : IAdminService
{
    private readonly EkubDbContext _db;
    private readonly INotificationService _notifications;

    public AdminService(EkubDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    private async Task EnsureAdminAsync(int adminId)
    {
        var user = await _db.Users.FindAsync(adminId)
            ?? throw new KeyNotFoundException("Admin user not found.");

        if (!user.IsAdmin)
        {
            throw new UnauthorizedAccessException("Only admin users can perform this action.");
        }
    }

    public async Task<AdminStatsDto> GetStatsAsync(int adminId)
    {
        await EnsureAdminAsync(adminId);

        var totalUsers = await _db.Users.CountAsync();
        var totalCircles = await _db.Circles.CountAsync();
        var activeCircles = await _db.Circles.CountAsync(c => c.Status == CircleStatus.Active);
        var formingCircles = await _db.Circles.CountAsync(c => c.Status == CircleStatus.Forming);
        var completedCircles = await _db.Circles.CountAsync(c => c.Status == CircleStatus.Completed);
        var totalCategories = await _db.EkubCategories.CountAsync();
        var totalSubCategories = await _db.EkubSubCategories.CountAsync();
        var pendingStories = await _db.SuccessStories.CountAsync(s => !s.IsApproved);
        var pendingFeedbacks = await _db.Feedbacks.CountAsync(f => f.Status == FeedbackStatus.Pending);
        var pendingSubscriptions = await _db.EkubSubscriptions.CountAsync(s => s.Status == SubscriptionStatus.PendingApproval);

        var totalSavingsVolume = await _db.EkubSubCategories
            .Where(s => s.Status == EkubSubCategoryStatus.Started || s.Status == EkubSubCategoryStatus.Completed)
            .SumAsync(s => s.TotalAmount);

        if (totalSavingsVolume == 0)
        {
            totalSavingsVolume = await _db.EkubSubCategories.SumAsync(s => s.TotalAmount);
        }

        return new AdminStatsDto(
            totalUsers,
            totalCircles,
            activeCircles,
            formingCircles,
            completedCircles,
            totalCategories,
            totalSubCategories,
            pendingStories,
            pendingFeedbacks,
            totalSavingsVolume,
            pendingSubscriptions
        );
    }

    public async Task<List<AdminUserDto>> GetUsersAsync(int adminId)
    {
        await EnsureAdminAsync(adminId);

        var users = await _db.Users
            .Include(u => u.CircleMemberships)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return users.Select(u => new AdminUserDto(
            u.Id,
            u.PhoneNumber,
            u.Email,
            u.FirstName,
            u.LastName,
            u.JobType,
            u.Location,
            u.IsPhoneVerified,
            u.IsAdmin,
            u.CircleMemberships.Count,
            u.CreatedAt
        )).ToList();
    }

    public async Task<AdminUserDto> ToggleUserAdminAsync(int adminId, int targetUserId)
    {
        await EnsureAdminAsync(adminId);

        if (adminId == targetUserId)
        {
            throw new InvalidOperationException("You cannot modify your own admin role.");
        }

        var targetUser = await _db.Users
            .Include(u => u.CircleMemberships)
            .FirstOrDefaultAsync(u => u.Id == targetUserId)
            ?? throw new KeyNotFoundException("Target user not found.");

        targetUser.IsAdmin = !targetUser.IsAdmin;
        targetUser.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new AdminUserDto(
            targetUser.Id,
            targetUser.PhoneNumber,
            targetUser.Email,
            targetUser.FirstName,
            targetUser.LastName,
            targetUser.JobType,
            targetUser.Location,
            targetUser.IsPhoneVerified,
            targetUser.IsAdmin,
            targetUser.CircleMemberships.Count,
            targetUser.CreatedAt
        );
    }

    public async Task<List<AdminStoryDto>> GetStoriesAsync(int adminId)
    {
        await EnsureAdminAsync(adminId);

        var stories = await _db.SuccessStories
            .Include(s => s.User)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return stories.Select(s => new AdminStoryDto(
            s.Id,
            s.UserId,
            s.AuthorName,
            s.User?.PhoneNumber,
            s.Content,
            s.Rating,
            s.IsApproved,
            s.CreatedAt
        )).ToList();
    }

    public async Task<AdminStoryDto> ApproveStoryAsync(int adminId, int storyId)
    {
        await EnsureAdminAsync(adminId);

        var story = await _db.SuccessStories
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == storyId)
            ?? throw new KeyNotFoundException("Success story not found.");

        story.IsApproved = true;
        await _db.SaveChangesAsync();

        return new AdminStoryDto(
            story.Id,
            story.UserId,
            story.AuthorName,
            story.User?.PhoneNumber,
            story.Content,
            story.Rating,
            story.IsApproved,
            story.CreatedAt
        );
    }

    public async Task DeleteStoryAsync(int adminId, int storyId)
    {
        await EnsureAdminAsync(adminId);

        var story = await _db.SuccessStories.FindAsync(storyId)
            ?? throw new KeyNotFoundException("Success story not found.");

        _db.SuccessStories.Remove(story);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AdminFeedbackDto>> GetFeedbacksAsync(int adminId)
    {
        await EnsureAdminAsync(adminId);

        var feedbacks = await _db.Feedbacks
            .Include(f => f.User)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return feedbacks.Select(f => new AdminFeedbackDto(
            f.Id,
            f.UserId,
            $"{f.User?.FirstName} {f.User?.LastName}".Trim(),
            f.User?.PhoneNumber ?? string.Empty,
            f.Subject,
            f.Message,
            f.Status,
            f.CreatedAt
        )).ToList();
    }

    public async Task<AdminFeedbackDto> UpdateFeedbackStatusAsync(int adminId, int feedbackId, FeedbackStatus status)
    {
        await EnsureAdminAsync(adminId);

        var feedback = await _db.Feedbacks
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.Id == feedbackId)
            ?? throw new KeyNotFoundException("Feedback not found.");

        feedback.Status = status;
        await _db.SaveChangesAsync();

        return new AdminFeedbackDto(
            feedback.Id,
            feedback.UserId,
            $"{feedback.User?.FirstName} {feedback.User?.LastName}".Trim(),
            feedback.User?.PhoneNumber ?? string.Empty,
            feedback.Subject,
            feedback.Message,
            feedback.Status,
            feedback.CreatedAt
        );
    }

    // --- Subscription / Membership Approvals ---

    public async Task<List<EkubSubscriptionDto>> GetSubscriptionsAsync(int adminId)
    {
        await EnsureAdminAsync(adminId);

        var subs = await _db.EkubSubscriptions
            .Include(s => s.SubCategory).ThenInclude(sc => sc!.Category)
            .Include(s => s.User)
            .OrderByDescending(s => s.SubmittedAt ?? s.JoinedAt)
            .ToListAsync();

        return subs.Select(MapToSubscriptionDto).ToList();
    }

    public async Task<EkubSubscriptionDto> ApproveSubscriptionAsync(int adminId, int subscriptionId)
    {
        await EnsureAdminAsync(adminId);

        var subscription = await _db.EkubSubscriptions
            .Include(s => s.SubCategory).ThenInclude(sc => sc!.Category)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId)
            ?? throw new KeyNotFoundException("Subscription application not found.");

        if (subscription.Status == SubscriptionStatus.Approved)
        {
            throw new InvalidOperationException("This subscription is already approved.");
        }

        var sub = subscription.SubCategory!;
        if (sub.CurrentMemberCount >= sub.MaxMembers)
        {
            throw new InvalidOperationException("This Ekub plan has already reached maximum capacity.");
        }

        subscription.Status = SubscriptionStatus.Approved;
        subscription.ApprovedAt = DateTime.UtcNow;
        subscription.RejectionReason = null;

        sub.CurrentMemberCount++;
        if (sub.CurrentMemberCount >= sub.MaxMembers)
        {
            sub.Status = EkubSubCategoryStatus.Full;
        }

        await _db.SaveChangesAsync();

        // Send congratulatory notification to user
        await _notifications.CreateAsync(
            subscription.UserId,
            NotificationType.MemberJoined,
            $"Membership Approved! 🎉",
            $"Your payment & National ID (FAN: {subscription.NationalIdFan}) for {sub.Name} have been verified. You are now officially joined in slot #{sub.CurrentMemberCount}!"
        );

        return MapToSubscriptionDto(subscription);
    }

    public async Task<EkubSubscriptionDto> RejectSubscriptionAsync(int adminId, int subscriptionId, string? reason)
    {
        await EnsureAdminAsync(adminId);

        var subscription = await _db.EkubSubscriptions
            .Include(s => s.SubCategory).ThenInclude(sc => sc!.Category)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId)
            ?? throw new KeyNotFoundException("Subscription application not found.");

        subscription.Status = SubscriptionStatus.Rejected;
        subscription.RejectionReason = reason ?? "Payment proof or National ID could not be verified.";

        await _db.SaveChangesAsync();

        var sub = subscription.SubCategory!;
        // Send rejection notification to user
        await _notifications.CreateAsync(
            subscription.UserId,
            NotificationType.General,
            $"Membership Verification Update: {sub.Name}",
            $"Your payment verification for {sub.Name} was not approved: {subscription.RejectionReason}. Please review your details and resubmit."
        );

        return MapToSubscriptionDto(subscription);
    }

    private static EkubSubscriptionDto MapToSubscriptionDto(EkubSubscription s)
    {
        var sub = s.SubCategory!;
        return new EkubSubscriptionDto(
            s.Id,
            s.UserId,
            s.User?.PhoneNumber ?? "",
            sub.Id,
            sub.Name,
            sub.Category?.Name ?? "",
            sub.DailyContribution,
            sub.TotalAmount,
            s.FullName,
            s.NationalIdFan,
            s.PaymentProofUrl,
            s.Status,
            s.RejectionReason,
            s.JoinedAt,
            s.SubmittedAt,
            s.ApprovedAt
        );
    }
}
