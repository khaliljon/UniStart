using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public enum NotificationConfirmResult
{
    Granted,
    NotFound,
    NotMatched,
    AlreadyProcessed,
    DuplicatePaymentId,
    OrderMissing,
}

/// <summary>
/// Turns raw payment evidence (from Gmail today, Kaspi API later) into reviewable
/// <c>KaspiPaymentNotification</c> records and drives admin confirmation through the existing
/// <see cref="IPaymentOrderService.ConfirmAndGrantAsync"/> — the single grant gate.
/// </summary>
public interface IKaspiPaymentEvidenceService
{
    /// <summary>Hangfire entry point: pull new evidence and evaluate it idempotently.</summary>
    Task PollAsync();

    Task<IReadOnlyList<KaspiNotificationDto>> ListAsync(IEnumerable<string> statuses);

    Task<NotificationConfirmResult> ConfirmAsync(int notificationId);
}
