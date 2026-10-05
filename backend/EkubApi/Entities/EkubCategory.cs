namespace EkubApi.Entities;

/// <summary>
/// A main Ekub category created by an admin, e.g., "Driver Ekub", "Trader Ekub",
/// "Workers Ekub", "Ye Ayinet Ekub", "Ye Frign Equb".
/// Each category holds one or more sub-categories (specific Ekub plans) that users can join.
/// </summary>
public class EkubCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<EkubSubCategory> SubCategories { get; set; } = [];
}
