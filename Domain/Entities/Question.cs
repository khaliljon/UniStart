namespace UniStart.Domain.Entities;

public class Question : ISoftDeletable, IAuditable
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionDifficulty Difficulty { get; set; }
    public bool IsMultipleChoice { get; set; } = false;
    public string? Explanation { get; set; }
    public string? Hint { get; set; }
    public string? VideoUrl { get; set; }
    public string? ImageUrl { get; set; }
    public int? ReadingPassageId { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public double DifficultyParam { get; set; } = 0.0;
    public double DiscriminationParam { get; set; } = 1.0;
    public double GuessParam { get; set; } = 0.25;
    public int ResponseCount { get; set; }
    public bool IsCalibrated { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public int? CreatedByTutorId { get; set; }
    public bool IsPrivate { get; set; }

    public virtual Topic Topic { get; set; } = null!;
    public virtual ReadingPassage? ReadingPassage { get; set; }
    public virtual User? CreatedByTutor { get; set; }
    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}

public enum QuestionDifficulty
{
    Easy = 1,
    Medium = 2,
    Hard = 3
}
