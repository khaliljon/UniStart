namespace UniStart.Domain.Entities;

/// <summary>
/// A user's attempt at a mock exam — tracks progress through sections.
/// </summary>
public class MockExamAttempt
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MockExamId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "in_progress"; // in_progress, completed, abandoned
    public int CurrentSectionIndex { get; set; } // 0-based
    public double? TotalScore { get; set; }
    public string? SectionScoresJson { get; set; } // JSON: [{ sectionName, correct, total, score }]
    public string? SelectedSectionIdsJson { get; set; } // JSON: [1,2,5] — subset of MockExamSection IDs (null = all)

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual MockExam MockExam { get; set; } = null!;
    public virtual ICollection<MockExamAnswer> Answers { get; set; } = new List<MockExamAnswer>();
}
