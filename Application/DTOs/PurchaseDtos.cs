using System.ComponentModel.DataAnnotations;

namespace UniStart.Application.DTOs;

public record PurchaseDto(
    int Id,
    string ItemType,
    string ItemCode,
    string Title,
    string? Subjects,
    decimal Amount,
    string Currency,
    string Status,
    DateTime PurchasedAt,
    string? PolarOrderId = null,
    string? CheckoutRef = null);

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

public record AdminSalesDto(
    int Count,
    decimal TotalRevenue,
    string Currency,
    IEnumerable<AdminPurchaseDto> Items,
    decimal TotalNet = 0);

public record PurchaseAmountsDto(
    decimal Gross,
    decimal Tax,
    decimal PlatformFee,
    string? PlatformFeeCurrency,
    decimal Net,
    decimal Total);
