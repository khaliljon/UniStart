namespace UniStart.Domain.Entities;

public class SupportTicket : IAuditable
{
    public int Id { get; set; }

    public long TelegramUserId { get; set; }

    public long TelegramChatId { get; set; }

    public string? Username { get; set; }
    public string? FirstName { get; set; }

    public string Status { get; set; } = "Open";

    public DateTime LastMessageAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
}
