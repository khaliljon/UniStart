namespace UniStart.Domain.Entities;

/// <summary>
/// User annotation on a mistake (wrong answer) with error classification and notes.
/// </summary>
public class UserMistakeNote : IAuditable
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int UserAnswerId { get; set; }
    public ErrorType? ErrorType { get; set; }
    public string? NoteText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual UserAnswer UserAnswer { get; set; } = null!;
}

public enum ErrorType
{
    Careless = 1,
    KnowledgeGap = 2,
    TimePressure = 3,
    TrickQuestion = 4
}
