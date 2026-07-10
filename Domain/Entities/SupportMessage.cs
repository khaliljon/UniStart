namespace UniStart.Domain.Entities;

/// <summary>
/// A single message within a support ticket (from the user or from support staff).
/// </summary>
public class SupportMessage : IAuditable
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    /// <summary>"In" (from user) | "Out" (from support).</summary>
    public string Direction { get; set; } = "In";

    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// For inbound messages: the message id of the copy forwarded to the support group.
    /// Operators reply to that message; the bot uses this to route the reply back to the user.
    /// </summary>
    public long? GroupMessageId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual SupportTicket Ticket { get; set; } = null!;
}
