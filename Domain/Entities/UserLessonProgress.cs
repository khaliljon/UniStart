namespace UniStart.Domain.Entities;

public class UserLessonProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LessonStepId { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
    public virtual LessonStep LessonStep { get; set; } = null!;
}
