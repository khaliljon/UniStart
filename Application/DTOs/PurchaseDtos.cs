using System.ComponentModel.DataAnnotations;

namespace UniStart.Application.DTOs;

/// <summary>A user's purchase record.</summary>
public record PurchaseDto(
    int Id,
    string ItemType,
    string ItemCode,
    string Title,
    string? Subjects,
    decimal Amount,
    string Currency,
    string Status,
    DateTime PurchasedAt);

/// <summary>Checkout request to create a purchase (payment via subscription stub).</summary>
public class CheckoutDto
{
    /// <summary>"package" | "book" | "course".</summary>
    [Required, MaxLength(30)]
    public string ItemType { get; set; } = string.Empty;

    [Required, MaxLength(60)]
    public string ItemCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Comma-separated subject codes (optional).</summary>
    [MaxLength(200)]
    public string? Subjects { get; set; }

    [Range(0, 100_000_000)]
    public decimal Amount { get; set; }

    [MaxLength(8)]
    public string Currency { get; set; } = "KZT";
}
