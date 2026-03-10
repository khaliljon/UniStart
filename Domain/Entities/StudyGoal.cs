namespace UniStart.Domain.Entities;

public class StudyGoal : IAuditable
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ExamTypeCode { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public int TargetScore { get; set; }
    public string? SelectedSectionIds { get; set; } // Comma-separated section IDs; null = all sections
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ExamType ExamType { get; set; } = null!;
    public virtual ICollection<StudyPlan> StudyPlans { get; set; } = new List<StudyPlan>();
}
