using TmsApi.Domain.Enums;

namespace TmsApi.Domain.Entities;

public class Circle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Contribution { get; set; }
    public string MeetingLabel { get; set; } = string.Empty;
    public CircleStatus Status { get; set; } = CircleStatus.Forming;
    public int OrganizerId { get; set; }
    public User? Organizer { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ICollection<CircleMember> Members { get; set; } = [];
    public ICollection<Round> Rounds { get; set; } = [];
}
