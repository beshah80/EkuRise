using EkubApi.DTOs;

namespace EkubApi.Services;

public interface IFeedbackService
{
    Task<FeedbackDto> SubmitFeedbackAsync(int userId, CreateFeedbackDto dto);
    Task<List<FeedbackDto>> GetMyFeedbacksAsync(int userId);
    Task<List<SuccessStoryDto>> GetApprovedSuccessStoriesAsync();
    Task<SuccessStoryDto> SubmitSuccessStoryAsync(int userId, CreateSuccessStoryDto dto);
}
