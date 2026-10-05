using EkubApi.Data;
using EkubApi.DTOs;
using EkubApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace EkubApi.Services;

public class FeedbackService : IFeedbackService
{
    private readonly EkubDbContext _db;

    public FeedbackService(EkubDbContext db)
    {
        _db = db;
    }

    public async Task<FeedbackDto> SubmitFeedbackAsync(int userId, CreateFeedbackDto dto)
    {
        var feedback = new Feedback
        {
            UserId = userId,
            Subject = dto.Subject,
            Message = dto.Message,
            CreatedAt = DateTime.UtcNow
        };
        _db.Feedbacks.Add(feedback);
        await _db.SaveChangesAsync();

        return MapToDto(feedback);
    }

    public async Task<List<FeedbackDto>> GetMyFeedbacksAsync(int userId)
    {
        var feedbacks = await _db.Feedbacks
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return feedbacks.Select(MapToDto).ToList();
    }

    public async Task<List<SuccessStoryDto>> GetApprovedSuccessStoriesAsync()
    {
        var stories = await _db.SuccessStories
            .Where(s => s.IsApproved)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return stories.Select(MapToStoryDto).ToList();
    }

    public async Task<SuccessStoryDto> SubmitSuccessStoryAsync(int userId, CreateSuccessStoryDto dto)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        var story = new SuccessStory
        {
            UserId = userId,
            AuthorName = dto.AuthorName ?? $"{user.FirstName} {user.LastName}",
            Content = dto.Content,
            Rating = dto.Rating,
            IsApproved = false, // Requires admin approval
            CreatedAt = DateTime.UtcNow
        };
        _db.SuccessStories.Add(story);
        await _db.SaveChangesAsync();

        return MapToStoryDto(story);
    }

    private static FeedbackDto MapToDto(Feedback f) => new(
        f.Id,
        f.Subject,
        f.Message,
        f.Status,
        f.CreatedAt
    );

    private static SuccessStoryDto MapToStoryDto(SuccessStory s) => new(
        s.Id,
        s.AuthorName,
        s.Content,
        s.Rating,
        s.CreatedAt
    );
}
