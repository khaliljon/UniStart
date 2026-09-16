namespace UniStart.Application.DTOs;

/// <summary>Admin view of a discovered Kaspi payment (from an email notification).</summary>
public record KaspiNotificationDto(
    int Id,
    string Status,
    string? OrderCode,
    string? KaspiPaymentId,
    decimal? Amount,
    string Currency,
    DateTime? PaidAt,
    DateTime ReceivedAt,
    string? ErrorMessage,
    int? UserId,
    string? UserName,
    string? UserEmail,
    decimal? ExpectedAmount,
    bool OrderFound,
    bool AmountMatches,
    bool PaymentIdUnique,
    string? ResolutionType = null,
    DateTime? ResolvedAt = null);
