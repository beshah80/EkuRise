namespace EkubApi.Entities;

/// <summary>
/// OTP verification code sent to a phone number during registration or login.
/// Codes expire after 10 minutes and can only be used once.
/// </summary>
public class PhoneVerification
{
    public int Id { get; set; }

    /// <summary>
    /// Nullable during registration (user record doesn't exist yet).
    /// </summary>
    public int? UserId { get; set; }
    public User? User { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// 6-digit verification code.
    /// </summary>
    public string VerificationCode { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
