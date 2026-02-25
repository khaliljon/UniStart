namespace UniStart.Domain.Entities;

public class Question
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionDifficulty Difficulty { get; set; }
    public string? Explanation { get; set; }
    public string? Hint { get; set; }
    public string? VideoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // IRT parameters (Item Response Theory)
    /// <summary>Difficulty parameter (b) on the logit scale, typically -3 to +3</summary>
    public double DifficultyParam { get; set; } = 0.0;
    /// <summary>Discrimination parameter (a) for 2PL model, typically 0.5 to 2.5</summary>
    public double DiscriminationParam { get; set; } = 1.0;
    /// <summary>Guessing parameter (c) for 3PL model, typically 0.0 to 0.35</summary>
    public double GuessParam { get; set; } = 0.25;

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
