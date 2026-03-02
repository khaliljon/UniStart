namespace UniStart.Domain.Entities;

/// <summary>
/// Records results from timed drill sessions (speed round, marathon, streak challenge).
/// </summary>
public class TimedDrillResult : IAuditable
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DrillType DrillType { get; set; }
    public string? ExamTypeCode { get; set; }
    public int? TopicId { get; set; }
    public int QuestionsAnswered { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalTimeSeconds { get; set; }
    public double AverageTimeSeconds { get; set; }
    public int BestStreak { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual ExamType? ExamType { get; set; }
    public virtual Topic? Topic { get; set; }
}

public enum DrillType
{
    Speed = 1,      // 10 questions, 60s each
    Marathon = 2,   // max questions in X minutes
    Streak = 3      // answer correctly until you miss
}
