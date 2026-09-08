namespace UniStart.Domain.Entities;

public class MockExamAttempt
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MockExamId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = "in_progress";
    public int CurrentSectionIndex { get; set; }
    public double? TotalScore { get; set; }
    public string? SectionScoresJson { get; set; }
    public string? SelectedSectionIdsJson { get; set; }
    public string Language { get; set; } = "en";
    public bool IsFree { get; set; }
    public string? AccessType { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual MockExam MockExam { get; set; } = null!;
    public virtual ICollection<MockExamAnswer> Answers { get; set; } = new List<MockExamAnswer>();
}
