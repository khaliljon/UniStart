namespace UniStart.Domain.Entities;

/// <summary>
/// An answer within a mock exam attempt — can be changed before section is completed.
/// </summary>
public class MockExamAnswer
{
    public int Id { get; set; }
    public int AttemptId { get; set; }
    public int QuestionId { get; set; }
    public int? SelectedOptionId { get; set; }
    public int SectionIndex { get; set; }
    public int SortOrder { get; set; }
    public int? TimeSpentSeconds { get; set; }
    public bool IsCorrect { get; set; }

    // Navigation properties
    public virtual MockExamAttempt Attempt { get; set; } = null!;
    public virtual Question Question { get; set; } = null!;
    public virtual AnswerOption? SelectedOption { get; set; }
}
