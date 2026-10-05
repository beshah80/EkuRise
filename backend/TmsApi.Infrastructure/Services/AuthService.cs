using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Domain.Enums;
using TmsApi.Infrastructure.Data;

namespace TmsApi.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config) { _db = db; _config = config; }

    public async Task<OtpSentDto> RegisterAsync(RegisterRequestDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.PhoneNumber == dto.PhoneNumber))
            throw new InvalidOperationException("This phone number is already registered.");

        int? referredById = null;
        if (!string.IsNullOrWhiteSpace(dto.ReferralCode))
        {
            var referrer = await _db.Users.FirstOrDefaultAsync(u => u.ReferralCode == dto.ReferralCode)
                ?? throw new InvalidOperationException("Invalid referral code.");
            referredById = referrer.Id;
        }

        var user = new User
        {
            PhoneNumber = dto.PhoneNumber, FirstName = dto.FirstName, LastName = dto.LastName,
            Gender = dto.Gender, JobType = dto.JobType, Location = dto.Location,
            IsPhoneVerified = false, ReferralCode = await GenerateReferralCodeAsync(dto.FirstName, dto.LastName),
            ReferredById = referredById, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var code = GenerateOtpCode();
        _db.PhoneVerifications.Add(new PhoneVerification { UserId = user.Id, PhoneNumber = dto.PhoneNumber, VerificationCode = code, ExpiresAt = DateTime.UtcNow.AddMinutes(10) });
        await _db.SaveChangesAsync();

        return new OtpSentDto(dto.PhoneNumber, "A verification code has been sent to your phone.", code);
    }

    public async Task<AuthResponseDto> VerifyRegistrationCodeAsync(VerifyRegistrationDto dto)
    {
        var v = await GetValidVerification(dto.PhoneNumber, dto.Code);
        v.IsUsed = true;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber)
            ?? throw new InvalidOperationException("No pending registration found.");
        user.IsPhoneVerified = true; user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return BuildAuthResponse(user);
    }

    public async Task<OtpSentDto> SendLoginOtpAsync(SendOtpDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber)
            ?? throw new InvalidOperationException("No account found with this phone number.");
        if (!user.IsPhoneVerified) throw new InvalidOperationException("Phone not verified.");

        var prev = await _db.PhoneVerifications.Where(p => p.PhoneNumber == dto.PhoneNumber && !p.IsUsed).ToListAsync();
        prev.ForEach(p => p.IsUsed = true);

        var code = GenerateOtpCode();
        _db.PhoneVerifications.Add(new PhoneVerification { UserId = user.Id, PhoneNumber = dto.PhoneNumber, VerificationCode = code, ExpiresAt = DateTime.UtcNow.AddMinutes(10) });
        await _db.SaveChangesAsync();
        return new OtpSentDto(dto.PhoneNumber, "A login code has been sent to your phone.", code);
    }

    public async Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpDto dto)
    {
        var v = await GetValidVerification(dto.PhoneNumber, dto.Code);
        v.IsUsed = true;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber)
            ?? throw new InvalidOperationException("Account not found.");
        await _db.SaveChangesAsync();
        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> PinLoginAsync(PinLoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber)
            ?? throw new InvalidOperationException("No account found with this phone number.");
        if (!user.IsPhoneVerified) throw new InvalidOperationException("Phone not verified.");
        if (!user.IsPinEnabled || string.IsNullOrEmpty(user.PinHash)) throw new InvalidOperationException("PIN login is not enabled.");
        if (!VerifyPin(dto.Pin, user.PinHash)) throw new UnauthorizedAccessException("Incorrect PIN.");
        return BuildAuthResponse(user);
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await GetUserOrThrow(userId);
        var referrerCode = user.ReferredById.HasValue ? (await _db.Users.FindAsync(user.ReferredById.Value))?.ReferralCode : null;
        return MapToProfileDto(user, referrerCode);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await GetUserOrThrow(userId);
        if (dto.Email is not null) user.Email = dto.Email;
        if (dto.FirstName is not null) user.FirstName = dto.FirstName;
        if (dto.LastName is not null) user.LastName = dto.LastName;
        if (dto.Gender.HasValue) user.Gender = dto.Gender.Value;
        if (dto.JobType is not null) user.JobType = dto.JobType;
        if (dto.Location is not null) user.Location = dto.Location;
        if (dto.ProfilePictureUrl is not null) user.ProfilePictureUrl = dto.ProfilePictureUrl;
        if (dto.PreferredLanguage.HasValue) user.PreferredLanguage = dto.PreferredLanguage.Value;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToProfileDto(user, null);
    }

    public async Task SetPinAsync(int userId, SetPinDto dto)
    {
        var user = await GetUserOrThrow(userId);
        if (user.IsPinEnabled && !string.IsNullOrEmpty(user.PinHash))
        {
            if (!VerifyPin(dto.CurrentCredential, user.PinHash)) throw new UnauthorizedAccessException("Current PIN is incorrect.");
        }
        else
        {
            var valid = await _db.PhoneVerifications.AnyAsync(p => p.PhoneNumber == user.PhoneNumber && p.VerificationCode == dto.CurrentCredential && !p.IsUsed && p.ExpiresAt > DateTime.UtcNow);
            if (!valid) throw new UnauthorizedAccessException("Invalid verification code.");
        }
        user.PinHash = HashPin(dto.Pin); user.IsPinEnabled = true; user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<OtpSentDto> RequestDeleteAccountAsync(DeleteAccountRequestDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber)
            ?? throw new InvalidOperationException("No account found.");
        var code = GenerateOtpCode();
        _db.AccountDeletionRequests.Add(new AccountDeletionRequest { UserId = user.Id, VerificationCode = code, ExpiresAt = DateTime.UtcNow.AddMinutes(10) });
        await _db.SaveChangesAsync();
        return new OtpSentDto(dto.PhoneNumber, "A verification code has been sent to confirm account deletion.", code);
    }

    public async Task ConfirmDeleteAccountAsync(int userId, ConfirmDeleteAccountDto dto)
    {
        var user = await GetUserOrThrow(userId);
        var req = await _db.AccountDeletionRequests.Where(a => a.UserId == userId && !a.IsConfirmed && a.ExpiresAt > DateTime.UtcNow).OrderByDescending(a => a.CreatedAt).FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No valid deletion request found.");
        if (req.VerificationCode != dto.Code) throw new UnauthorizedAccessException("Incorrect verification code.");
        req.IsConfirmed = true;
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        return user is null ? null : new UserDto(user.Id, $"{user.FirstName} {user.LastName}", user.PhoneNumber, user.ProfilePictureUrl);
    }

    private async Task<PhoneVerification> GetValidVerification(string phone, string code)
        => await _db.PhoneVerifications.Where(p => p.PhoneNumber == phone && p.VerificationCode == code && !p.IsUsed && p.ExpiresAt > DateTime.UtcNow).OrderByDescending(p => p.CreatedAt).FirstOrDefaultAsync()
            ?? throw new UnauthorizedAccessException("Invalid or expired verification code.");

    private async Task<User> GetUserOrThrow(int id)
        => await _db.Users.FindAsync(id) ?? throw new KeyNotFoundException("User not found.");

    private static string GenerateOtpCode() => RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

    private async Task<string> GenerateReferralCodeAsync(string first, string last)
    {
        var initials = $"{first[..Math.Min(2, first.Length)]}{last[..Math.Min(2, last.Length)]}".ToUpper();
        string code;
        do { code = $"EK-{initials}-{RandomNumberGenerator.GetInt32(1000, 10000)}"; }
        while (await _db.Users.AnyAsync(u => u.ReferralCode == code));
        return code;
    }

    private static string HashPin(string pin)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(pin, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(hash)}.{Convert.ToBase64String(salt)}.100000";
    }

    private static bool VerifyPin(string pin, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 3) return false;
        var hash = Convert.FromBase64String(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var iterations = int.Parse(parts[2]);
        var computed = Rfc2898DeriveBytes.Pbkdf2(pin, salt, iterations, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(hash, computed);
    }

    private AuthResponseDto BuildAuthResponse(User user) => new(GenerateToken(user), user.Id, user.FirstName, user.LastName, user.PhoneNumber, user.IsPhoneVerified, user.IsAdmin);

    private string GenerateToken(User user)
    {
        var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("isAdmin", user.IsAdmin.ToString().ToLowerInvariant())
        };
        var token = new JwtSecurityToken(_config["Jwt:Issuer"] ?? "TmsApi", _config["Jwt:Audience"] ?? "TmsClient", claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserProfileDto MapToProfileDto(User u, string? referrerCode) => new(u.Id, u.PhoneNumber, u.Email, u.FirstName, u.LastName, u.Gender, u.JobType, u.Location, u.ProfilePictureUrl, u.IsPhoneVerified, u.IsPinEnabled, u.PreferredLanguage, u.ReferralCode, referrerCode, u.IsAdmin, u.CreatedAt);
}
