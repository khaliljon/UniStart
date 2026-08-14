namespace UniStart.Domain.Entities;

public class MockExam
{
    public int Id { get; set; }
    public string ExamTypeCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string? TitleKz { get; set; }
    public string? TitleEn { get; set; }
    public string? DescriptionKz { get; set; }
    public string? DescriptionEn { get; set; }

    public int TotalTimeMinutes { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ExamType ExamType { get; set; } = null!;
    public virtual ICollection<MockExamSection> Sections { get; set; } = new List<MockExamSection>();
    public virtual ICollection<MockExamAttempt> Attempts { get; set; } = new List<MockExamAttempt>();
}
