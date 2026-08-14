namespace UniStart.Domain.Entities;

public class ReadingPassage
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public virtual Topic Topic { get; set; } = null!;
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
