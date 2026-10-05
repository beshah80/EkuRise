using EkubApi.Data;
using EkubApi.DTOs;
using EkubApi.Entities;
using EkubApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace EkubApi.Services;

public class CatalogService : ICatalogService
{
    private readonly EkubDbContext _db;
    private readonly INotificationService _notifications;

    public CatalogService(EkubDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    // --- Admin: Categories ---

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, int adminId)
    {
        await EnsureAdminAsync(adminId);

        var category = new EkubCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            IconUrl = dto.IconUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _db.EkubCategories.Add(category);
        await _db.SaveChangesAsync();

        return new CategoryDto(category.Id, category.Name, category.Description, category.IconUrl, 0);
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync(int userId)
    {
        var categories = await _db.EkubCategories
            .Where(c => c.IsActive)
            .Include(c => c.SubCategories)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.IconUrl,
            c.SubCategories.Count(s => s.Status != EkubSubCategoryStatus.Completed)
        )).ToList();
    }

    // --- Admin: Sub-Categories ---

    public async Task<SubCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto dto, int adminId)
    {
        await EnsureAdminAsync(adminId);

        var category = await _db.EkubCategories.FindAsync(dto.CategoryId)
            ?? throw new KeyNotFoundException("Category not found.");

        var totalAmount = dto.DailyContribution * dto.TotalRounds;

        var subCategory = new EkubSubCategory
        {
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            DailyContribution = dto.DailyContribution,
            TotalRounds = dto.TotalRounds,
            TotalAmount = totalAmount,
            StartDate = dto.StartDate,
            TermsAndConditions = dto.TermsAndConditions,
            MaxMembers = dto.MaxMembers,
            CurrentMemberCount = 0,
            Status = EkubSubCategoryStatus.Open,
            CreatedByAdminId = adminId,
            CreatedAt = DateTime.UtcNow
        };
        _db.EkubSubCategories.Add(subCategory);
        await _db.SaveChangesAsync();

        return MapToSubCategoryDto(subCategory, category.Name, false);
    }

    public async Task StartEkubAsync(int subCategoryId, int adminId)
    {
        await EnsureAdminAsync(adminId);

        var subCategory = await _db.EkubSubCategories
            .Include(s => s.Subscriptions)
            .FirstOrDefaultAsync(s => s.Id == subCategoryId)
            ?? throw new KeyNotFoundException("Sub-category not found.");

        if (subCategory.Status != EkubSubCategoryStatus.Open && subCategory.Status != EkubSubCategoryStatus.Full)
        {
            throw new InvalidOperationException("This Ekub has already been started or completed.");
        }

        // Only approved subscribers become circle members
        var subscribers = subCategory.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Approved)
            .ToList();

        if (subscribers.Count < 2)
        {
            throw new InvalidOperationException("Need at least 2 approved members with verified payments to start an Ekub.");
        }

        // Create a Circle
        var circle = new Circle
        {
            Name = subCategory.Name,
            Contribution = subCategory.DailyContribution,
            MeetingLabel = "Daily",
            Status = CircleStatus.Active,
            OrganizerId = subCategory.CreatedByAdminId,
            CreatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow
        };
        _db.Circles.Add(circle);
        await _db.SaveChangesAsync();

        // Add all approved subscribers as members with payout order by approval time
        var sortedSubs = subscribers.OrderBy(s => s.ApprovedAt ?? s.JoinedAt).ToList();
        for (var i = 0; i < sortedSubs.Count; i++)
        {
            var member = new CircleMember
            {
                CircleId = circle.Id,
                UserId = sortedSubs[i].UserId,
                PayoutOrder = i + 1,
                HasReceived = false,
                JoinedAt = sortedSubs[i].JoinedAt
            };
            _db.CircleMembers.Add(member);
        }
        await _db.SaveChangesAsync();

        // Create rounds (one per member), first one Open
        for (var i = 0; i < sortedSubs.Count; i++)
        {
            var round = new Round
            {
                CircleId = circle.Id,
                RoundNumber = i + 1,
                Status = i == 0 ? RoundStatus.Open : RoundStatus.Pending,
                OpenedAt = i == 0 ? DateTime.UtcNow : DateTime.MinValue
            };
            _db.Rounds.Add(round);
        }
        await _db.SaveChangesAsync();

        // Create payment rows for the first (open) round
        var firstRoundId = await _db.Rounds
            .Where(r => r.CircleId == circle.Id && r.Status == RoundStatus.Open)
            .Select(r => r.Id)
            .FirstAsync();

        var memberIds = sortedSubs.Select(s => s.UserId).ToList();
        var payments = memberIds.Select(uid => new Payment
        {
            RoundId = firstRoundId,
            UserId = uid,
            HasPaid = false,
            LateFine = 0m
        }).ToList();
        _db.Payments.AddRange(payments);

        // Link sub-category to circle and update status
        subCategory.CircleId = circle.Id;
        subCategory.Status = EkubSubCategoryStatus.Started;
        await _db.SaveChangesAsync();
    }

    // --- User: Browse ---

    public async Task<List<SubCategoryDto>> GetSubCategoriesAsync(int categoryId, int userId)
    {
        var subCategories = await _db.EkubSubCategories
            .Where(s => s.CategoryId == categoryId)
            .Include(s => s.Category)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        // Check which ones the user has joined (Approved status)
        var joinedIds = await _db.EkubSubscriptions
            .Where(sub => sub.UserId == userId && sub.Status == SubscriptionStatus.Approved && subCategories.Select(s => s.Id).Contains(sub.SubCategoryId))
            .Select(sub => sub.SubCategoryId)
            .ToHashSetAsync();

        return subCategories.Select(s => MapToSubCategoryDto(s, s.Category?.Name ?? "", joinedIds.Contains(s.Id))).ToList();
    }

    public async Task<SubCategoryDetailDto?> GetSubCategoryByIdAsync(int subCategoryId, int userId)
    {
        var sub = await _db.EkubSubCategories
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == subCategoryId);
        if (sub is null) return null;

        var subscription = await _db.EkubSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SubCategoryId == subCategoryId);

        var hasJoined = subscription is not null && subscription.Status == SubscriptionStatus.Approved;

        return new SubCategoryDetailDto(
            sub.Id,
            sub.CategoryId,
            sub.Category?.Name ?? "",
            sub.Name,
            sub.DailyContribution,
            sub.TotalRounds,
            sub.TotalAmount,
            sub.StartDate,
            sub.TermsAndConditions,
            sub.MaxMembers,
            sub.CurrentMemberCount,
            sub.Status,
            hasJoined,
            sub.CircleId,
            subscription?.Id,
            subscription?.Status,
            subscription?.FullName,
            subscription?.NationalIdFan,
            subscription?.PaymentProofUrl,
            subscription?.RejectionReason
        );
    }

    // --- User: Join & Submit Payment Proof ---

    public async Task<JoinResultDto> JoinSubCategoryAsync(int subCategoryId, bool agreedToTerms, int userId)
    {
        var sub = await _db.EkubSubCategories
            .FirstOrDefaultAsync(s => s.Id == subCategoryId)
            ?? throw new KeyNotFoundException("Ekub not found.");

        if (sub.Status != EkubSubCategoryStatus.Open)
        {
            throw new InvalidOperationException("This Ekub is not open for joining.");
        }

        if (sub.CurrentMemberCount >= sub.MaxMembers)
        {
            throw new InvalidOperationException("This Ekub is full.");
        }

        if (!agreedToTerms)
        {
            throw new InvalidOperationException("You must agree to the terms and conditions to join.");
        }

        // Check if already applied or joined
        var existing = await _db.EkubSubscriptions
            .FirstOrDefaultAsync(sub => sub.UserId == userId && sub.SubCategoryId == subCategoryId);

        if (existing is not null)
        {
            if (existing.Status == SubscriptionStatus.Approved)
            {
                throw new InvalidOperationException("You have already joined and been verified for this Ekub.");
            }
            if (existing.Status == SubscriptionStatus.PendingApproval)
            {
                throw new InvalidOperationException("Your payment proof and National ID are currently under Admin review.");
            }

            // If in PendingPayment or Rejected, return existing subscription so user can proceed
            return new JoinResultDto(
                existing.Id,
                sub.Id,
                sub.Name,
                sub.DailyContribution,
                sub.TotalAmount,
                sub.StartDate,
                "Please proceed to payment and submit your National ID (FAN) and screenshot.",
                existing.Status
            );
        }

        var subscription = new EkubSubscription
        {
            UserId = userId,
            SubCategoryId = subCategoryId,
            AgreedToTerms = true,
            Status = SubscriptionStatus.PendingPayment,
            JoinedAt = DateTime.UtcNow
        };
        _db.EkubSubscriptions.Add(subscription);
        await _db.SaveChangesAsync();

        // Create notification for the user to proceed to payment
        await _notifications.CreateAsync(
            userId,
            NotificationType.PaymentReminder,
            $"Proceed to Payment: {sub.Name}",
            $"You applied to join {sub.Name}. Please submit your National ID (FAN) and payment screenshot ({sub.DailyContribution:N0} ETB) to verify your slot."
        );

        return new JoinResultDto(
            subscription.Id,
            sub.Id,
            sub.Name,
            sub.DailyContribution,
            sub.TotalAmount,
            sub.StartDate,
            "Application submitted! Please proceed to submit your payment proof and National ID (FAN).",
            subscription.Status
        );
    }

    public async Task<EkubSubscriptionDto> SubmitPaymentProofAsync(int subscriptionId, SubmitPaymentProofDto dto, int userId)
    {
        var subscription = await _db.EkubSubscriptions
            .Include(s => s.SubCategory).ThenInclude(sc => sc!.Category)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId && s.UserId == userId)
            ?? throw new KeyNotFoundException("Subscription not found.");

        if (subscription.Status == SubscriptionStatus.Approved)
        {
            throw new InvalidOperationException("This subscription is already approved and active.");
        }

        subscription.FullName = dto.FullName.Trim();
        subscription.NationalIdFan = dto.NationalIdFan.Trim();
        subscription.PaymentProofUrl = dto.PaymentProofUrl;
        subscription.Status = SubscriptionStatus.PendingApproval;
        subscription.SubmittedAt = DateTime.UtcNow;
        subscription.RejectionReason = null;

        await _db.SaveChangesAsync();

        var sub = subscription.SubCategory!;

        // Notify user that submission was received
        await _notifications.CreateAsync(
            userId,
            NotificationType.General,
            $"Payment Submitted for {sub.Name}",
            $"Your payment proof and National ID (FAN: {subscription.NationalIdFan}) were received and are awaiting Admin approval."
        );

        return new EkubSubscriptionDto(
            subscription.Id,
            subscription.UserId,
            subscription.User?.PhoneNumber ?? "",
            sub.Id,
            sub.Name,
            sub.Category?.Name ?? "",
            sub.DailyContribution,
            sub.TotalAmount,
            subscription.FullName,
            subscription.NationalIdFan,
            subscription.PaymentProofUrl,
            subscription.Status,
            subscription.RejectionReason,
            subscription.JoinedAt,
            subscription.SubmittedAt,
            subscription.ApprovedAt
        );
    }

    // --- User: My Ekubs ---

    public async Task<List<MyEkubDto>> GetMyEkubsAsync(int userId)
    {
        var subscriptions = await _db.EkubSubscriptions
            .Where(s => s.UserId == userId)
            .Include(s => s.SubCategory).ThenInclude(sc => sc!.Category)
            .OrderByDescending(s => s.JoinedAt)
            .ToListAsync();

        return subscriptions.Select(s =>
        {
            var sub = s.SubCategory!;
            return new MyEkubDto(
                s.Id,
                sub.Id,
                sub.Category?.Name ?? "",
                sub.Name,
                sub.DailyContribution,
                sub.TotalAmount,
                sub.StartDate,
                sub.Status,
                s.JoinedAt,
                sub.CircleId,
                s.Status
            );
        }).ToList();
    }

    // --- Helpers ---

    private async Task EnsureAdminAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");
        if (!user.IsAdmin)
        {
            throw new UnauthorizedAccessException("Only admin users can perform this action.");
        }
    }

    private static SubCategoryDto MapToSubCategoryDto(EkubSubCategory s, string categoryName, bool hasJoined)
    {
        return new SubCategoryDto(
            s.Id,
            s.CategoryId,
            categoryName,
            s.Name,
            s.DailyContribution,
            s.TotalRounds,
            s.TotalAmount,
            s.StartDate,
            s.MaxMembers,
            s.CurrentMemberCount,
            s.Status,
            hasJoined
        );
    }
}
