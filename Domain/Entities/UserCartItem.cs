namespace UniStart.Domain.Entities;

/// <summary>
/// A cart line stored server-side so the cart follows the user across devices.
/// Mirrors the client cart item shape.
/// </summary>
public class UserCartItem : IAuditable
{
    public int Id { get; set; }

    public int UserId { get; set; }

    /// <summary>"mock" | "package" | "book".</summary>
    public string ItemType { get; set; } = string.Empty;

    /// <summary>Mock id, package key, or subject key.</summary>
    public string ItemCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    /// <summary>Comma-separated subject codes (package selection / book subject).</summary>
    public string? Subjects { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "KZT";

    /// <summary>Runs for a mock line (null otherwise).</summary>
    public int? Runs { get; set; }

    /// <summary>Comma-separated selected mock ids for a package line.</summary>
    public string? SelectedMockIds { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
