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

    // ── Polar order breakdown (populated from the webhook; 0 for pre-Polar rows) ──
    /// <summary>Subtotal reported by Polar (order currency, e.g. KZT), allocated per line.</summary>
    public decimal GrossAmount { get; set; }
    /// <summary>Tax computed by Polar for the buyer's country (order currency).</summary>
    public decimal TaxAmount { get; set; }
    /// <summary>Polar platform fee. NOTE: charged in <see cref="PlatformFeeCurrency"/> (often USD), not the order currency.</summary>
    public decimal PlatformFeeAmount { get; set; }
    /// <summary>Currency of the platform fee (e.g. "usd") — do not sum with order-currency amounts.</summary>
    public string? PlatformFeeCurrency { get; set; }
    /// <summary>Net payout amount reported by Polar (order currency).</summary>
    public decimal NetAmount { get; set; }
    /// <summary>Total charged to the buyer incl. tax (order currency).</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>"Paid" | "Pending" | "Cancelled".</summary>
    public string Status { get; set; } = "Paid";

    public DateTime PurchasedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
