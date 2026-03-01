namespace UniStart.Domain.Entities;

public class Conversation
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TutorId { get; set; }

    // Denormalized for fast list rendering
    public string? LastMessagePreview { get; set; }   // ≤100
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCountStudent { get; set; } = 0;
    public int UnreadCountTutor { get; set; } = 0;

    public ConversationStatus Status { get; set; } = ConversationStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual User Student { get; set; } = null!;
    public virtual User Tutor { get; set; } = null!;
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

public enum ConversationStatus
{
    Active,
    Archived
}
