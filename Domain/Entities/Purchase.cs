namespace UniStart.Domain.Entities;

/// <summary>
/// A record of something a user bought (mock-exam package, book or course).
/// Created at checkout. Payment is currently handled via the subscription stub;
/// this table gives users an order history ("Мои покупки").
/// </summary>
public class Purchase : IAuditable
{
    public int Id { get; set; }

    public int UserId { get; set; }

    /// <summary>"package" | "book" | "course".</summary>
    public string ItemType { get; set; } = string.Empty;

    /// <summary>Stable code of the item, e.g. package key "standard" or subject code.</summary>
    public string ItemCode { get; set; } = string.Empty;

    /// <summary>Human-readable label snapshot at purchase time.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Comma-separated subject codes the purchase grants (optional).</summary>
    public string? Subjects { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "KZT";

    /// <summary>"Paid" | "Pending" | "Cancelled".</summary>
    public string Status { get; set; } = "Paid";

    public DateTime PurchasedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
