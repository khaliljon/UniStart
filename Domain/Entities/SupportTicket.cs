namespace UniStart.Domain.Entities;

/// <summary>
/// A support conversation started by a Telegram user via the support bot.
/// One ticket per Telegram user; messages are stored in <see cref="SupportMessage"/>.
/// </summary>
public class SupportTicket : IAuditable
{
    public int Id { get; set; }

    /// <summary>Telegram user id (numeric).</summary>
    public long TelegramUserId { get; set; }

    /// <summary>Telegram private chat id used to send replies back (== user id for private chats).</summary>
    public long TelegramChatId { get; set; }

    public string? Username { get; set; }
    public string? FirstName { get; set; }

    /// <summary>"Open" | "Closed".</summary>
    public string Status { get; set; } = "Open";

    public DateTime LastMessageAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
}
