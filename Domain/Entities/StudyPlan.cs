namespace UniStart.Domain.Entities;

public class StudyPlan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int GoalId { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual StudyGoal Goal { get; set; } = null!;
    public virtual ICollection<StudyPlanEntry> Entries { get; set; } = new List<StudyPlanEntry>();
}
