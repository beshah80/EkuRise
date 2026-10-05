namespace TmsApi.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int RoundId { get; set; }
    public Round? Round { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public bool HasPaid { get; set; }
    public DateTime? PaidAt { get; set; }
    public decimal LateFine { get; set; } = 0m;
}
