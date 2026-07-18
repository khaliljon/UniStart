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
    DateTime PurchasedAt,
    decimal GrossAmount = 0,
    decimal TaxAmount = 0,
    decimal PlatformFeeAmount = 0,
    string? PlatformFeeCurrency = null,
    decimal NetAmount = 0,
    decimal TotalAmount = 0);

/// <summary>Aggregated sales view for the admin panel.</summary>
public record AdminSalesDto(
    int Count,
    decimal TotalRevenue,
    string Currency,
    IEnumerable<AdminPurchaseDto> Items,
    decimal TotalNet = 0);

/// <summary>Polar order money breakdown, taken from the webhook payload (already ÷100).</summary>
public record PurchaseAmountsDto(
    decimal Gross,
    decimal Tax,
    decimal PlatformFee,
    string? PlatformFeeCurrency,
    decimal Net,
    decimal Total);
