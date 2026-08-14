namespace UniStart.Domain.Entities;

public class StrategyGuide : IAuditable
{
    public int Id { get; set; }
    public string ExamTypeCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "test-taking";
    public int EstimatedReadMinutes { get; set; } = 5;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ExamType ExamType { get; set; } = null!;
    public virtual ICollection<UserGuideProgress> UserProgress { get; set; } = new List<UserGuideProgress>();
}
