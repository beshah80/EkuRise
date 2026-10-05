namespace EkubApi.Entities;

/// <summary>
/// Request to delete an account. The user must verify via a code sent to their phone.
/// Only after verification is the account actually deleted.
/// </summary>
public class AccountDeletionRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public string VerificationCode { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
