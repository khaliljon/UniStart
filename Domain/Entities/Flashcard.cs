namespace UniStart.Domain.Entities;

public class Flashcard : IAuditable
{
    public int Id { get; set; }
    public int DeckId { get; set; }
    public string Front { get; set; } = string.Empty;  // question side (markdown)
    public string Back { get; set; } = string.Empty;    // answer side (markdown)
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual FlashcardDeck Deck { get; set; } = null!;
    public virtual ICollection<UserFlashcardProgress> UserProgress { get; set; } = new List<UserFlashcardProgress>();
}
