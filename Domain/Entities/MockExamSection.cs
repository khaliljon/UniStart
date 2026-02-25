namespace UniStart.Domain.Entities;

/// <summary>
/// A section within a mock exam — has its own time limit, instructions, and question pool.
/// </summary>
public class MockExamSection
{
    public int Id { get; set; }
    public int MockExamId { get; set; }
    public int? ExamSectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; }
    public int SortOrder { get; set; }
    public string? Instructions { get; set; }

    // Navigation properties
    public virtual MockExam MockExam { get; set; } = null!;
    public virtual ExamSection? ExamSection { get; set; }
}
