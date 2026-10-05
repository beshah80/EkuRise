using System.ComponentModel.DataAnnotations;
using TmsApi.Domain.Enums;

namespace TmsApi.Application.DTOs;

public record RegisterRequestDto
{
    [Required] [StringLength(20, MinimumLength = 10)] public string PhoneNumber { get; init; } = string.Empty;
    [Required] [StringLength(100, MinimumLength = 2)] public string FirstName { get; init; } = string.Empty;
    [Required] [StringLength(100, MinimumLength = 2)] public string LastName { get; init; } = string.Empty;
    [Required] public Gender Gender { get; init; }
    [Required] [StringLength(100)] public string JobType { get; init; } = string.Empty;
    [Required] [StringLength(200)] public string Location { get; init; } = string.Empty;
    public string? ReferralCode { get; init; }
}

public record VerifyRegistrationDto
{
    [Required] public string PhoneNumber { get; init; } = string.Empty;
    [Required] [StringLength(6, MinimumLength = 6)] public string Code { get; init; } = string.Empty;
}

public record SendOtpDto { [Required] public string PhoneNumber { get; init; } = string.Empty; }

public record VerifyOtpDto
{
    [Required] public string PhoneNumber { get; init; } = string.Empty;
    [Required] [StringLength(6, MinimumLength = 6)] public string Code { get; init; } = string.Empty;
}

public record PinLoginDto
{
    [Required] public string PhoneNumber { get; init; } = string.Empty;
    [Required] [StringLength(6, MinimumLength = 4)] public string Pin { get; init; } = string.Empty;
}

public record SetPinDto
{
    [Required] [StringLength(6, MinimumLength = 4)] public string Pin { get; init; } = string.Empty;
    [Required] public string CurrentCredential { get; init; } = string.Empty;
}

public record UpdateProfileDto
{
    [StringLength(256)] public string? Email { get; init; }
    [StringLength(100, MinimumLength = 2)] public string? FirstName { get; init; }
    [StringLength(100, MinimumLength = 2)] public string? LastName { get; init; }
    public Gender? Gender { get; init; }
    [StringLength(100)] public string? JobType { get; init; }
    [StringLength(200)] public string? Location { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public Language? PreferredLanguage { get; init; }
}

public record DeleteAccountRequestDto { [Required] public string PhoneNumber { get; init; } = string.Empty; }
public record ConfirmDeleteAccountDto { [Required] [StringLength(6, MinimumLength = 6)] public string Code { get; init; } = string.Empty; }

public record AuthResponseDto(string Token, int UserId, string FirstName, string LastName, string PhoneNumber, bool IsPhoneVerified, bool IsAdmin);
public record OtpSentDto(string PhoneNumber, string Message, string? DemoCode = null);
public record UserProfileDto(int Id, string PhoneNumber, string? Email, string FirstName, string LastName, Gender Gender, string JobType, string Location, string? ProfilePictureUrl, bool IsPhoneVerified, bool IsPinEnabled, Language PreferredLanguage, string ReferralCode, string? ReferredByCode, bool IsAdmin, DateTime CreatedAt);
public record UserDto(int Id, string FullName, string PhoneNumber, string? ProfilePictureUrl);
