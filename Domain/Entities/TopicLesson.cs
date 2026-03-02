namespace UniStart.Domain.Entities;

public class TopicLesson
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public int SortOrder { get; set; }

    // Navigation
    public virtual Topic Topic { get; set; } = null!;
    public virtual ICollection<LessonStep> Steps { get; set; } = new List<LessonStep>();
}
