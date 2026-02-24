namespace UniStart.Domain.Entities;

public class UserSkillProfile
{
    public int UserId { get; set; }
    public int SkillId { get; set; }
    public int Level { get; set; } = 50; // Display level (0-100)
    
    // IRT-based ability estimation
    /// <summary>Latent ability parameter θ (theta) on logit scale, typically -4 to +4</summary>
    public double Theta { get; set; } = 0.0;
    /// <summary>Standard error of θ estimate — smaller = more confident</summary>
    public double ThetaSE { get; set; } = 1.0;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}
