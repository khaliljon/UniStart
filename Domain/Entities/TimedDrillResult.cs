namespace UniStart.Domain.Entities;

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

    public virtual User User { get; set; } = null!;
    public virtual ExamType? ExamType { get; set; }
    public virtual Topic? Topic { get; set; }
}

public enum DrillType
{
    Speed = 1,
    Marathon = 2,
    Streak = 3
}
