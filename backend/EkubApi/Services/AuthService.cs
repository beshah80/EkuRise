using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EkubApi.Data;
using EkubApi.DTOs;
using EkubApi.Entities;
using EkubApi.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EkubApi.Services;

public class AuthService : IAuthService
{
    private readonly EkubDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(EkubDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // --- Registration ---

    /// <summary>
    /// Full registration flow: creates a user with IsPhoneVerified=false,
    /// generates an OTP, and returns the code for demo purposes.
    /// </summary>
    public async Task<OtpSentDto> RegisterAsync(RegisterRequestDto dto)
    {
        var existing = await _db.Users.AnyAsync(u => u.PhoneNumber == dto.PhoneNumber);
        if (existing)
        {
            throw new InvalidOperationException("This phone number is already registered.");
        }

        // Resolve referral
        int? referredById = null;
        if (!string.IsNullOrWhiteSpace(dto.ReferralCode))
        {
            var referrer = await _db.Users.FirstOrDefaultAsync(u => u.ReferralCode == dto.ReferralCode);
            if (referrer is null)
            {
                throw new InvalidOperationException("Invalid referral code.");
            }
            referredById = referrer.Id;
        }

        // Create user (unverified)
        var user = new User
        {
            PhoneNumber = dto.PhoneNumber,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Gender = dto.Gender,
            JobType = dto.JobType,
            Location = dto.Location,
            IsPhoneVerified = false,
            ReferralCode = await GenerateReferralCodeAsync(dto.FirstName, dto.LastName),
            ReferredById = referredById,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Generate OTP
        var code = GenerateOtpCode();
        var verification = new PhoneVerification
        {
            UserId = user.Id,
            PhoneNumber = dto.PhoneNumber,
            VerificationCode = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.PhoneVerifications.Add(verification);
        await _db.SaveChangesAsync();

        return new OtpSentDto(
            dto.PhoneNumber,
            "A verification code has been sent to your phone.",
            code
        );
    }

    /// <summary>
    /// Verify the OTP sent during registration and activate the account.
    /// </summary>
    public async Task<AuthResponseDto> VerifyRegistrationCodeAsync(VerifyRegistrationDto dto)
    {
        var verification = await GetValidVerification(dto.PhoneNumber, dto.Code);
        verification.IsUsed = true;

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber)
            ?? throw new InvalidOperationException("No pending registration found for this phone number.");

        user.IsPhoneVerified = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new AuthResponseDto(
            GenerateToken(user),
            user.Id,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.IsPhoneVerified,
            user.IsAdmin
        );
    }

    // --- Login ---

    public async Task<OtpSentDto> SendLoginOtpAsync(SendOtpDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber)
            ?? throw new InvalidOperationException("No account found with this phone number. Please register first.");

        if (!user.IsPhoneVerified)
        {
            throw new InvalidOperationException("Your phone number is not verified. Please complete registration.");
        }

        // Invalidate previous codes
        var previousCodes = await _db.PhoneVerifications
            .Where(pv => pv.PhoneNumber == dto.PhoneNumber && !pv.IsUsed)
            .ToListAsync();
        foreach (var pv in previousCodes) pv.IsUsed = true;

        var code = GenerateOtpCode();
        var verification = new PhoneVerification
        {
            UserId = user.Id,
            PhoneNumber = dto.PhoneNumber,
            VerificationCode = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.PhoneVerifications.Add(verification);
        await _db.SaveChangesAsync();

        return new OtpSentDto(
            dto.PhoneNumber,
            "A login code has been sent to your phone.",
            code
        );
    }

    public async Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpDto dto)
    {
        var verification = await GetValidVerification(dto.PhoneNumber, dto.Code);
        verification.IsUsed = true;

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber)
            ?? throw new InvalidOperationException("Account not found.");

        await _db.SaveChangesAsync();

        return new AuthResponseDto(
            GenerateToken(user),
            user.Id,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.IsPhoneVerified,
            user.IsAdmin
        );
    }

    public async Task<AuthResponseDto> PinLoginAsync(PinLoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber)
            ?? throw new InvalidOperationException("No account found with this phone number.");

        if (!user.IsPhoneVerified)
        {
            throw new InvalidOperationException("Your phone number is not verified.");
        }

        if (!user.IsPinEnabled || string.IsNullOrEmpty(user.PinHash))
        {
            throw new InvalidOperationException("PIN login is not enabled. Set a PIN in your account settings first.");
        }

        if (!VerifyPin(dto.Pin, user.PinHash))
        {
            throw new UnauthorizedAccessException("Incorrect PIN.");
        }

        return new AuthResponseDto(
            GenerateToken(user),
            user.Id,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.IsPhoneVerified,
            user.IsAdmin
        );
    }

    // --- Account management ---

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await GetUserOrThrow(userId);
        var referrerCode = user.ReferredById.HasValue
            ? (await _db.Users.FindAsync(user.ReferredById.Value))?.ReferralCode
            : null;

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

        // Verify current credential (PIN or OTP code that was sent)
        // For the hackathon, we'll accept the current PIN or any recent unused OTP
        if (user.IsPinEnabled && !string.IsNullOrEmpty(user.PinHash))
        {
            if (!VerifyPin(dto.CurrentCredential, user.PinHash))
            {
                throw new UnauthorizedAccessException("Current PIN is incorrect.");
            }
        }
        else
        {
            // For first-time PIN setup, accept any recent valid OTP for this phone
            var validOtp = await _db.PhoneVerifications
                .AnyAsync(pv => pv.PhoneNumber == user.PhoneNumber
                    && pv.VerificationCode == dto.CurrentCredential
                    && !pv.IsUsed
                    && pv.ExpiresAt > DateTime.UtcNow);
            if (!validOtp)
            {
                throw new UnauthorizedAccessException("Invalid verification code. Please verify your phone first.");
            }
        }

        user.PinHash = HashPin(dto.Pin);
        user.IsPinEnabled = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<OtpSentDto> RequestDeleteAccountAsync(DeleteAccountRequestDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber)
            ?? throw new InvalidOperationException("No account found with this phone number.");

        if (!user.IsPhoneVerified)
        {
            throw new InvalidOperationException("Account is not verified.");
        }

        var code = GenerateOtpCode();
        var request = new AccountDeletionRequest
        {
            UserId = user.Id,
            VerificationCode = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.AccountDeletionRequests.Add(request);
        await _db.SaveChangesAsync();

        return new OtpSentDto(
            dto.PhoneNumber,
            "A verification code has been sent to confirm account deletion.",
            code
        );
    }

    public async Task ConfirmDeleteAccountAsync(int userId, ConfirmDeleteAccountDto dto)
    {
        var user = await GetUserOrThrow(userId);

        var request = await _db.AccountDeletionRequests
            .Where(a => a.UserId == userId && !a.IsConfirmed && a.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No valid deletion request found. Please request a new code.");

        if (request.VerificationCode != dto.Code)
        {
            throw new UnauthorizedAccessException("Incorrect verification code.");
        }

        request.IsConfirmed = true;

        // Delete the user (cascading deletes handle related data)
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return null;

        return new UserDto(
            user.Id,
            $"{user.FirstName} {user.LastName}",
            user.PhoneNumber,
            user.ProfilePictureUrl
        );
    }

    // --- Helpers ---

    private async Task<PhoneVerification> GetValidVerification(string phoneNumber, string code)
    {
        var verification = await _db.PhoneVerifications
            .Where(pv => pv.PhoneNumber == phoneNumber
                && pv.VerificationCode == code
                && !pv.IsUsed
                && pv.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(pv => pv.CreatedAt)
            .FirstOrDefaultAsync()
            ?? throw new UnauthorizedAccessException("Invalid or expired verification code.");

        return verification;
    }

    private async Task<User> GetUserOrThrow(int userId)
    {
        return await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");
    }

    private static string GenerateOtpCode()
    {
        return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
    }

    private async Task<string> GenerateReferralCodeAsync(string firstName, string lastName)
    {
        var initials = $"{firstName[..Math.Min(2, firstName.Length)]}{lastName[..Math.Min(2, lastName.Length)]}".ToUpper();
        var prefix = $"EK-{initials}";
        // Ensure uniqueness
        var code = $"{prefix}-{RandomNumberGenerator.GetInt32(1000, 10000)}";
        while (await _db.Users.AnyAsync(u => u.ReferralCode == code))
        {
            code = $"{prefix}-{RandomNumberGenerator.GetInt32(1000, 10000)}";
        }
        return code;
    }

    private static string HashPin(string pin)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(pin, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(hash)}.{Convert.ToBase64String(salt)}.{100_000}";
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

    private string GenerateToken(User user)
    {
        var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
        var issuer = _config["Jwt:Issuer"] ?? "EkubApi";
        var audience = _config["Jwt:Audience"] ?? "EkubClient";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}"),
            new Claim("phone", user.PhoneNumber),
            new Claim("isAdmin", user.IsAdmin.ToString().ToLowerInvariant()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserProfileDto MapToProfileDto(User user, string? referrerCode)
    {
        return new UserProfileDto(
            user.Id,
            user.PhoneNumber,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Gender,
            user.JobType,
            user.Location,
            user.ProfilePictureUrl,
            user.IsPhoneVerified,
            user.IsPinEnabled,
            user.PreferredLanguage,
            user.ReferralCode,
            referrerCode,
            user.IsAdmin,
            user.CreatedAt
        );
    }
}
