namespace UniStart.Domain.Entities;

/// <summary>
/// Per-user ability estimate for one exam Section (subject). Replaces the former
/// per-Skill profile after the taxonomy was simplified to Exam → Section → Topic → Question.
/// </summary>
public class UserSkillProfile
{
    public int UserId { get; set; }
    public int SectionId { get; set; }
    public int Level { get; set; } = 50; // Display level (0-100)
    
    // IRT-based ability estimation
    /// <summary>Latent ability parameter θ (theta) on logit scale, typically -4 to +4</summary>
    public double Theta { get; set; } = 0.0;
    /// <summary>Standard error of θ estimate — smaller = more confident</summary>
    public double ThetaSE { get; set; } = 1.0;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ExamSection Section { get; set; } = null!;
}
