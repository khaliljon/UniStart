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
