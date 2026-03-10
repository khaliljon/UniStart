namespace UniStart.Domain.Entities;

/// <summary>
/// Admin-configurable drill templates that students can pick from.
/// </summary>
public class DrillTemplate : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DrillType DrillType { get; set; }
    public string? ExamTypeCode { get; set; }
    public int? TopicId { get; set; }
    public int QuestionCount { get; set; } = 10;
    public int? TimeLimitMinutes { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual ExamType? ExamType { get; set; }
    public virtual Topic? Topic { get; set; }
}
