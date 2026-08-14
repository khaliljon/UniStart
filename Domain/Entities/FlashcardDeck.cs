namespace UniStart.Domain.Entities;

public class FlashcardDeck : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ExamTypeCode { get; set; }
    public int? TopicId { get; set; }
    public bool IsSystem { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ExamType? ExamType { get; set; }
    public virtual Topic? Topic { get; set; }
    public virtual User? CreatedByUser { get; set; }
    public virtual ICollection<Flashcard> Cards { get; set; } = new List<Flashcard>();
}
