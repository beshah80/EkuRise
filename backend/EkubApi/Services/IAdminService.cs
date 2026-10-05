using EkubApi.DTOs;
using EkubApi.Enums;

namespace EkubApi.Services;

public interface IAdminService
{
    Task<AdminStatsDto> GetStatsAsync(int adminId);
    Task<List<AdminUserDto>> GetUsersAsync(int adminId);
    Task<AdminUserDto> ToggleUserAdminAsync(int adminId, int targetUserId);
    Task<List<AdminStoryDto>> GetStoriesAsync(int adminId);
    Task<AdminStoryDto> ApproveStoryAsync(int adminId, int storyId);
    Task DeleteStoryAsync(int adminId, int storyId);
    Task<List<AdminFeedbackDto>> GetFeedbacksAsync(int adminId);
    Task<AdminFeedbackDto> UpdateFeedbackStatusAsync(int adminId, int feedbackId, FeedbackStatus status);
}
