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
    AmountMismatch,
}

public enum NotificationRejectResult
{
    Rejected,
    NotFound,
    NotReviewable,
}

public enum NotificationActionResult
{
    Done,
    NotFound,
    NotApplicable,
    AlreadyResolved,
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

    /// <summary>Dismisses a RequiresReview notification (→ Rejected). Never touches the order/grant.</summary>
    Task<NotificationRejectResult> RejectAsync(int notificationId);

    /// <summary>Emails the buyer that the paid amount didn't match (amount-mismatch review only). No state change.</summary>
    Task<NotificationActionResult> NotifyUserOfMismatchAsync(int notificationId);

    /// <summary>Records a manual full refund for an amount-mismatch review item. Never grants access.</summary>
    Task<NotificationActionResult> MarkRefundedAsync(int notificationId, int adminUserId);
}
