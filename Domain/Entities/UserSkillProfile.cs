namespace UniStart.Domain.Entities;

public class UserSkillProfile
{
    public int UserId { get; set; }
    public int SkillId { get; set; }
    public int Level { get; set; } = 50; // Default skill level (0-100)
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}
