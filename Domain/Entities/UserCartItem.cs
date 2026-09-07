namespace UniStart.Domain.Entities;

/// <summary>
/// A cart line stored server-side so the cart follows the user across devices.
/// Mirrors the client cart item shape.
/// </summary>
public class UserCartItem : IAuditable
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ItemType { get; set; } = string.Empty;

    public string ItemCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Subjects { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "KZT";

    public int? Runs { get; set; }

    public string? SelectedMockIds { get; set; }

    public string? Language { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
