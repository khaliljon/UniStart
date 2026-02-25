namespace UniStart.Domain.Entities;

/// <summary>
/// Reading passage for TOEFL-style questions — multiple questions linked to one passage.
/// </summary>
public class ReadingPassage
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Passage text (markdown)
    public int SortOrder { get; set; }

    // Navigation properties
    public virtual Topic Topic { get; set; } = null!;
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
