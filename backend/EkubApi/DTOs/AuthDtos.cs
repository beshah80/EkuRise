using System.ComponentModel.DataAnnotations;
using EkubApi.Enums;

namespace EkubApi.DTOs;

/// <summary>
/// Step 1 of registration: user enters phone + profile data.
/// API sends an OTP to the phone (returned in response for demo).
/// </summary>
public record RegisterRequestDto
{
    [Required(ErrorMessage = "Phone number is required")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "Enter a valid phone number")]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Gender is required")]
    public Gender Gender { get; init; }

    [Required(ErrorMessage = "Job type is required")]
    [StringLength(100)]
    public string JobType { get; init; } = string.Empty;

    [Required(ErrorMessage = "Location is required")]
    [StringLength(200)]
    public string Location { get; init; } = string.Empty;

    /// <summary>
    /// Optional referral code from another user.
    /// </summary>
    public string? ReferralCode { get; init; }
}

/// <summary>
/// Step 2 of registration: user enters the OTP code sent to their phone.
/// </summary>
public record VerifyRegistrationDto
{
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Request to send an OTP for login (phone number only).
/// </summary>
public record SendOtpDto
{
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; init; } = string.Empty;
}

/// <summary>
/// Verify OTP for login.
/// </summary>
public record VerifyOtpDto
{
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Login with phone + PIN (if user has set a PIN).
/// </summary>
public record PinLoginDto
{
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "PIN is required")]
    [StringLength(6, MinimumLength = 4, ErrorMessage = "PIN must be 4-6 digits")]
    public string Pin { get; init; } = string.Empty;
}

/// <summary>
/// Set or change the login PIN.
/// </summary>
public record SetPinDto
{
    [Required(ErrorMessage = "PIN is required")]
    [StringLength(6, MinimumLength = 4, ErrorMessage = "PIN must be 4-6 digits")]
    public string Pin { get; init; } = string.Empty;

    [Required(ErrorMessage = "Current password or PIN is required for verification")]
    public string CurrentCredential { get; init; } = string.Empty;
}

/// <summary>
/// Response after successful authentication.
/// </summary>
public record AuthResponseDto(
    string Token,
    int UserId,
    string FirstName,
    string LastName,
    string PhoneNumber,
    bool IsPhoneVerified,
    bool IsAdmin
);

/// <summary>
/// Response when an OTP is sent (includes the code for demo/development).
/// </summary>
public record OtpSentDto(
    string PhoneNumber,
    string Message,
    string? DemoCode = null
);

/// <summary>
/// Full user profile shown in Account screen.
/// </summary>
public record UserProfileDto(
    int Id,
    string PhoneNumber,
    string? Email,
    string FirstName,
    string LastName,
    Gender Gender,
    string JobType,
    string Location,
    string? ProfilePictureUrl,
    bool IsPhoneVerified,
    bool IsPinEnabled,
    Language PreferredLanguage,
    string ReferralCode,
    string? ReferredByCode,
    bool IsAdmin,
    DateTime CreatedAt
);

/// <summary>
/// Update user profile fields.
/// </summary>
public record UpdateProfileDto
{
    [StringLength(256)]
    public string? Email { get; init; }

    [StringLength(100, MinimumLength = 2)]
    public string? FirstName { get; init; }

    [StringLength(100, MinimumLength = 2)]
    public string? LastName { get; init; }

    public Gender? Gender { get; init; }

    [StringLength(100)]
    public string? JobType { get; init; }

    [StringLength(200)]
    public string? Location { get; init; }

    public string? ProfilePictureUrl { get; init; }

    public Language? PreferredLanguage { get; init; }
}

/// <summary>
/// Request to delete account (step 1: triggers a verification code).
/// </summary>
public record DeleteAccountRequestDto
{
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; init; } = string.Empty;
}

/// <summary>
/// Confirm account deletion with the code sent to the phone.
/// </summary>
public record ConfirmDeleteAccountDto
{
    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Basic user info shown to other members in a circle.
/// </summary>
public record UserDto(
    int Id,
    string FullName,
    string PhoneNumber,
    string? ProfilePictureUrl
);
