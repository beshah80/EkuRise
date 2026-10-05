using EkubApi.DTOs;

namespace EkubApi.Services;

public interface IAuthService
{
    // Registration
    Task<OtpSentDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> VerifyRegistrationCodeAsync(VerifyRegistrationDto dto);

    // Login
    Task<OtpSentDto> SendLoginOtpAsync(SendOtpDto dto);
    Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpDto dto);
    Task<AuthResponseDto> PinLoginAsync(PinLoginDto dto);

    // Account
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    Task SetPinAsync(int userId, SetPinDto dto);
    Task<OtpSentDto> RequestDeleteAccountAsync(DeleteAccountRequestDto dto);
    Task ConfirmDeleteAccountAsync(int userId, ConfirmDeleteAccountDto dto);

    // Lookup
    Task<UserDto?> GetUserByIdAsync(int userId);
}
