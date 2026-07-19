namespace UniStart.Domain.Entities;

/// <summary>
/// Mock exam template — defines a full exam simulation with sections and time limits.
/// </summary>
public class MockExam
{
    public int Id { get; set; }
    public string ExamTypeCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>Kazakh title (optional; falls back to <see cref="Title"/>).</summary>
    public string? TitleKz { get; set; }
    /// <summary>English title (optional; falls back to <see cref="Title"/>).</summary>
    public string? TitleEn { get; set; }
    /// <summary>Kazakh description (optional; falls back to <see cref="Description"/>).</summary>
    public string? DescriptionKz { get; set; }
    /// <summary>English description (optional; falls back to <see cref="Description"/>).</summary>
    public string? DescriptionEn { get; set; }

    public int TotalTimeMinutes { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ExamType ExamType { get; set; } = null!;
    public virtual ICollection<MockExamSection> Sections { get; set; } = new List<MockExamSection>();
    public virtual ICollection<MockExamAttempt> Attempts { get; set; } = new List<MockExamAttempt>();
}
