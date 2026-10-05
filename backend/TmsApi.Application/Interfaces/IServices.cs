using TmsApi.Application.DTOs;
using TmsApi.Domain.Enums;

namespace TmsApi.Application.Interfaces;

public interface IAuthService
{
    Task<OtpSentDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> VerifyRegistrationCodeAsync(VerifyRegistrationDto dto);
    Task<OtpSentDto> SendLoginOtpAsync(SendOtpDto dto);
    Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpDto dto);
    Task<AuthResponseDto> PinLoginAsync(PinLoginDto dto);
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task SetPinAsync(int userId, SetPinDto dto);
    Task<OtpSentDto> RequestDeleteAccountAsync(DeleteAccountRequestDto dto);
    Task ConfirmDeleteAccountAsync(int userId, ConfirmDeleteAccountDto dto);
    Task<UserDto?> GetUserByIdAsync(int userId);
}

public interface ICircleService
{
    Task<CircleDetailDto> CreateCircleAsync(CreateCircleDto dto, int organizerId);
    Task<List<CircleSummaryDto>> GetMyCirclesAsync(int userId);
    Task<CircleDetailDto?> GetCircleByIdAsync(int circleId, int userId);
    Task<MemberDto> AddMemberAsync(int circleId, AddMemberDto dto, int organizerId);
    Task RemoveMemberAsync(int circleId, int userId, int organizerId);
    Task<CircleDetailDto> StartCircleAsync(int circleId, int organizerId);
}

public interface IRoundService
{
    Task<List<RoundSummaryDto>> GetRoundsAsync(int circleId, int? roundNumber, RoundStatus? status);
    Task<RoundDetailDto?> GetRoundByIdAsync(int circleId, int roundId, int userId);
    Task<RoundDetailDto?> GetCurrentRoundAsync(int circleId, int userId);
    Task<RoundDetailDto> MarkPaymentAsync(int circleId, int roundId, MarkPaymentDto dto, int organizerId);
    Task<PayoutResultDto> PayOutAsync(int circleId, int roundId, int organizerId);
    Task<RoundDetailDto> OpenNextRoundAsync(int circleId, int organizerId);
}

public interface INotificationService
{
    Task<List<NotificationDto>> GetNotificationsAsync(int userId);
    Task MarkAsReadAsync(int userId, int notificationId);
    Task MarkAllReadAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task CreateAsync(int userId, NotificationType type, string title, string body, int? circleId = null, int? roundId = null);
}

public interface ICatalogService
{
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, int adminId);
    Task<List<CategoryDto>> GetCategoriesAsync(int userId);
    Task<SubCategoryDto> CreateSubCategoryAsync(CreateSubCategoryDto dto, int adminId);
    Task StartEkubAsync(int subCategoryId, int adminId);
    Task<List<SubCategoryDto>> GetSubCategoriesAsync(int categoryId, int userId);
    Task<SubCategoryDetailDto?> GetSubCategoryByIdAsync(int subCategoryId, int userId);
    Task<JoinResultDto> JoinSubCategoryAsync(int subCategoryId, bool agreedToTerms, int userId);
    Task<List<MyEkubDto>> GetMyEkubsAsync(int userId);
}

public interface IFeedbackService
{
    Task<FeedbackDto> SubmitFeedbackAsync(int userId, CreateFeedbackDto dto);
    Task<List<FeedbackDto>> GetMyFeedbacksAsync(int userId);
    Task<List<SuccessStoryDto>> GetApprovedSuccessStoriesAsync();
    Task<SuccessStoryDto> SubmitSuccessStoryAsync(int userId, CreateSuccessStoryDto dto);
}
