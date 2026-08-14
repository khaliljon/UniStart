namespace UniStart.Domain.Entities;

public class UserSkillProfile
{
    public int UserId { get; set; }
    public int SectionId { get; set; }
    public int Level { get; set; } = 50;
    
    public double Theta { get; set; } = 0.0;
    public double ThetaSE { get; set; } = 1.0;
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
    public virtual ExamSection Section { get; set; } = null!;
}
