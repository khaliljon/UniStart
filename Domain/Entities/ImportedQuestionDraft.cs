namespace UniStart.Domain.Entities;

public class ImportedQuestionDraft
{
    public int Id { get; set; }
    public int ImportJobId { get; set; }
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>JSON array of answer options: [{"text":"...", "isCorrect": true/false}, ...]</summary>
    public string OptionsJson { get; set; } = "[]";

    public string? Explanation { get; set; }
    public string? Hint { get; set; }
    public int? TopicId { get; set; }
    public QuestionDifficulty Difficulty { get; set; } = QuestionDifficulty.Medium;

    // IRT parameters (auto-assigned defaults)
    public double IrtA { get; set; } = 1.0;
    public double IrtB { get; set; } = 0.0;
    public double IrtC { get; set; } = 0.25;

    public DraftStatus Status { get; set; } = DraftStatus.Pending;
    public DraftSource Source { get; set; } = DraftSource.Exact;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedByUserId { get; set; }

    // Navigation properties
    public virtual QuestionImportJob ImportJob { get; set; } = null!;
    public virtual Topic? Topic { get; set; }
}

public enum DraftStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public enum DraftSource
{
    Exact = 0,
    Analog = 1
}
