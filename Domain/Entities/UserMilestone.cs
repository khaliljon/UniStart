namespace UniStart.Domain.Entities;

public class UserMilestone
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;   // e.g. "Q100", "MASTERY_ALGEBRA", "STREAK_7"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "🏆";
    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual User User { get; set; } = null!;
}
