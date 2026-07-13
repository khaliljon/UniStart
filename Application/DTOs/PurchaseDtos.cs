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

/// <summary>Purchase row for the admin sales monitor (includes buyer info).</summary>
public record AdminPurchaseDto(
    int Id,
    int UserId,
    string UserName,
    string UserEmail,
    string ItemType,
    string ItemCode,
    string Title,
    string? Subjects,
    decimal Amount,
    string Currency,
    string Status,
    DateTime PurchasedAt);

/// <summary>Aggregated sales view for the admin panel.</summary>
public record AdminSalesDto(
    int Count,
    decimal TotalRevenue,
    string Currency,
    IEnumerable<AdminPurchaseDto> Items);

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
