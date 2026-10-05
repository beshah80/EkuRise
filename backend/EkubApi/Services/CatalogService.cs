using EkubApi.Data;
using EkubApi.DTOs;
using EkubApi.Entities;
using EkubApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace EkubApi.Services;

public class CatalogService : ICatalogService
{
    private readonly EkubDbContext _db;

    public CatalogService(EkubDbContext db)
    {
        _db = db;
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
            MaxMembers = dto.MaxMembers > 0 ? dto.MaxMembers : dto.TotalRounds,
            CurrentMemberCount = 0,
            Status = EkubSubCategoryStatus.Open,
            CreatedByAdminId = adminId,
            CreatedAt = DateTime.UtcNow
        };
        _db.EkubSubCategories.Add(subCategory);
        await _db.SaveChangesAsync();

        // Auto-subscribe the admin (they are the organizer but also a member)
        var subscription = new EkubSubscription
        {
            UserId = adminId,
            SubCategoryId = subCategory.Id,
            AgreedToTerms = true,
            JoinedAt = DateTime.UtcNow
        };
        _db.EkubSubscriptions.Add(subscription);
        subCategory.CurrentMemberCount = 1;
        await _db.SaveChangesAsync();

        return MapToSubCategoryDto(subCategory, category.Name, false);
    }

    /// <summary>
    /// Admin starts the Ekub: creates a Circle from all subscribed members,
    /// auto-starts it (locks members, creates rounds, sets payout order).
    /// </summary>
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

        var subscribers = subCategory.Subscriptions.ToList();
        if (subscribers.Count < 2)
        {
            throw new InvalidOperationException("Need at least 2 members to start an Ekub.");
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

        // Add all subscribers as members with payout order by join time
        var sortedSubs = subscribers.OrderBy(s => s.JoinedAt).ToList();
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

        // Check which ones the user has joined
        var joinedIds = await _db.EkubSubscriptions
            .Where(sub => sub.UserId == userId && subCategories.Select(s => s.Id).Contains(sub.SubCategoryId))
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

        var hasJoined = await _db.EkubSubscriptions
            .AnyAsync(sub => sub.UserId == userId && sub.SubCategoryId == subCategoryId);

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
            sub.CircleId
        );
    }

    // --- User: Join ---

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

        // Check if already joined
        var existing = await _db.EkubSubscriptions
            .AnyAsync(sub => sub.UserId == userId && sub.SubCategoryId == subCategoryId);
        if (existing)
        {
            throw new InvalidOperationException("You have already joined this Ekub.");
        }

        var subscription = new EkubSubscription
        {
            UserId = userId,
            SubCategoryId = subCategoryId,
            AgreedToTerms = true,
            JoinedAt = DateTime.UtcNow
        };
        _db.EkubSubscriptions.Add(subscription);

        sub.CurrentMemberCount++;
        if (sub.CurrentMemberCount >= sub.MaxMembers)
        {
            sub.Status = EkubSubCategoryStatus.Full;
        }

        await _db.SaveChangesAsync();

        return new JoinResultDto(
            subscription.Id,
            sub.Id,
            sub.Name,
            sub.DailyContribution,
            sub.TotalAmount,
            sub.StartDate,
            "You have successfully joined this Ekub!"
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
                sub.CircleId
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
