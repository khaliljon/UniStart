namespace UniStart.Domain.Entities;

public class UserGuideProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int GuideId { get; set; }
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual StrategyGuide Guide { get; set; } = null!;
}
