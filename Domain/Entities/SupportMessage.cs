namespace UniStart.Domain.Entities;

public class SupportMessage : IAuditable
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public string Direction { get; set; } = "In";

    public string Text { get; set; } = string.Empty;

    public long? GroupMessageId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual SupportTicket Ticket { get; set; } = null!;
}
