namespace UniStart.Domain.Entities;

public class FormulaCard : IAuditable
{
    public int Id { get; set; }
    public int TopicId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Formula { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual Topic Topic { get; set; } = null!;
    public virtual ICollection<UserFormulaBookmark> Bookmarks { get; set; } = new List<UserFormulaBookmark>();
}
