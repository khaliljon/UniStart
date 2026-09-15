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
    string? CheckoutRef = null,
    string? PaymentProvider = null,
    string? ExternalPaymentId = null,
    string? OrderCode = null);

/// <summary>Admin request to manually confirm a Kaspi order after verifying the Kaspi register.</summary>
public record KaspiConfirmDto(
    [property: Required] string KaspiPaymentId,
    [property: Required] decimal PaidAmount,
    string? Note = null);

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
    decimal TotalAmount = 0,
    string? PaymentProvider = null,
    string? ExternalPaymentId = null,
    string? OrderCode = null,
    string? PolarOrderId = null,
    string? CheckoutRef = null);

/// <summary>A payment-provider fee aggregate, kept per currency (never mix currencies).</summary>
public record SalesFeeDto(string Provider, string Currency, decimal Amount);

public record AdminSalesDto(
    decimal TotalRevenue,
    string Currency,
    int PaidOrders,
    IEnumerable<SalesFeeDto> Fees,
    IEnumerable<AdminPurchaseDto> Items);

public record PurchaseAmountsDto(
    decimal Gross,
    decimal Tax,
    decimal PlatformFee,
    string? PlatformFeeCurrency,
    decimal Net,
    decimal Total);
