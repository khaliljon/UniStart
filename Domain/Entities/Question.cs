namespace UniStart.Domain.Entities;

public class Question
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionDifficulty Difficulty { get; set; }
    public string? Explanation { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Topic Topic { get; set; } = null!;
    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}

public enum QuestionDifficulty
{
    Easy = 1,
    Medium = 2,
    Hard = 3
}
