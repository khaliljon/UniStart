namespace UniStart.Domain.Entities;

public class Message
{
    public long Id { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string Text { get; set; } = string.Empty;   // ≤4000
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public bool IsEdited { get; set; } = false;
    public MessageType Type { get; set; } = MessageType.Text;

    // Navigation
    public virtual Conversation Conversation { get; set; } = null!;
    public virtual User Sender { get; set; } = null!;
}

public enum MessageType
{
    Text,
    System
}
