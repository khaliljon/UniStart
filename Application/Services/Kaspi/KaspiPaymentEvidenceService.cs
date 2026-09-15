using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services.Kaspi;

public partial class KaspiPaymentEvidenceService : IKaspiPaymentEvidenceService
{
    private readonly UniStartDbContext _db;
    private readonly IPaymentEvidenceSource _source;
    private readonly IPaymentOrderService _orders;
    private readonly ILogger<KaspiPaymentEvidenceService> _logger;
    private readonly int _overlapMinutes;

    private const string CheckpointKey = "kaspi:gmail:checkpoint";

    [GeneratedRegex("^US-K-[0-9A-Z]{8}$")]
    private static partial Regex OrderCodeFormat();

    public KaspiPaymentEvidenceService(
        UniStartDbContext db,
        IPaymentEvidenceSource source,
        IPaymentOrderService orders,
        IConfiguration config,
        ILogger<KaspiPaymentEvidenceService> logger)
    {
        _db = db;
        _source = source;
        _orders = orders;
        _logger = logger;
        _overlapMinutes = config.GetValue<int?>("GMAIL_KASPI_OVERLAP_MINUTES") ?? 10;
    }

    public async Task PollAsync()
    {
        if (!_source.Enabled) return;

        var checkpoint = await ReadCheckpointAsync();
        // At-least-once: replay a small overlap window; correctness comes from the unique
        // GmailMessageId, not from checkpoint precision.
        DateTime? since = checkpoint.HasValue ? checkpoint.Value.AddMinutes(-_overlapMinutes) : null;

        IReadOnlyList<KaspiPaymentEvidence> items;
        try
        {
            items = await _source.FetchNewAsync(since);
        }
        catch (Exception ex)
        {
            // Let Hangfire retry; never break API startup or payment flow.
            _logger.LogError(ex, "Kaspi Gmail poll failed while fetching evidence");
            throw;
        }

        var maxReceived = checkpoint ?? DateTime.MinValue;
        foreach (var ev in items.OrderBy(e => e.ReceivedAt))
        {
            if (ev.ReceivedAt > maxReceived) maxReceived = ev.ReceivedAt;

            // Hard idempotency: one source event is processed once.
            if (await _db.KaspiPaymentNotifications.AnyAsync(n => n.Source == ev.Source && n.SourceEventId == ev.SourceMessageId))
                continue;

            await ProcessEvidenceAsync(ev);
        }

        if (maxReceived > (checkpoint ?? DateTime.MinValue))
            await WriteCheckpointAsync(maxReceived);
    }

    private const string TrustedPaymentIdIndex = "IX_KaspiPaymentNotifications_KaspiPaymentId_Trusted";
    private const string SourceEventIndex = "IX_KaspiPaymentNotifications_Source_SourceEventId";

    private async Task ProcessEvidenceAsync(KaspiPaymentEvidence ev)
    {
        var (status, orderId, reason) = await EvaluateAsync(ev);
        await InsertNotificationAsync(ev, status, orderId, reason);
    }

    private async Task InsertNotificationAsync(KaspiPaymentEvidence ev, string status, int? orderId, string? reason)
    {
        var notification = new KaspiPaymentNotification
        {
            Source = ev.Source,
            SourceEventId = ev.SourceMessageId,
            KaspiPaymentId = ev.KaspiPaymentId,
            OrderCode = ev.OrderCode,
            Amount = ev.Amount,
            Currency = string.IsNullOrWhiteSpace(ev.Currency) ? "KZT" : ev.Currency,
            PaidAt = ev.PaidAt,
            ReceivedAt = ev.ReceivedAt,
            Status = status,
            PaymentOrderId = orderId,
            ErrorMessage = reason,
        };

        _db.KaspiPaymentNotifications.Add(notification);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _db.Entry(notification).State = EntityState.Detached;
            var constraint = (ex.InnerException as PostgresException)?.ConstraintName;

            // DB is the final race guard: the payment id was matched concurrently — downgrade to review.
            if (string.Equals(constraint, TrustedPaymentIdIndex, StringComparison.Ordinal)
                && status == KaspiNotificationStatuses.Matched)
            {
                _logger.LogWarning("Payment id {PaymentId} matched concurrently; downgrading evidence {Source}/{EventId} to review",
                    ev.KaspiPaymentId, ev.Source, ev.SourceMessageId);
                await InsertNotificationAsync(ev, KaspiNotificationStatuses.RequiresReview, orderId, "payment id already matched (concurrency)");
                return;
            }

            // Duplicate (Source, SourceEventId) — same event already processed.
            _logger.LogWarning(ex, "Duplicate Kaspi evidence {Source}/{EventId} ignored (constraint {Constraint})",
                ev.Source, ev.SourceMessageId, constraint ?? "unknown");
        }
    }

    // Returns (status, matchedOrderId, reason). Never grants access.
    private async Task<(string Status, int? OrderId, string? Reason)> EvaluateAsync(KaspiPaymentEvidence ev)
    {
        if (ev.ParseError != null)
            return (KaspiNotificationStatuses.RequiresReview, null, ev.ParseError);

        // Do NOT trust an unauthenticated email's payment id for matching.
        if (ev.Auth != EmailAuthVerdict.Pass)
            return (KaspiNotificationStatuses.RequiresReview, null, $"email authentication not passed ({ev.AuthReason ?? "unknown"})");

        if (ev.OrderCode == null || !OrderCodeFormat().IsMatch(ev.OrderCode))
            return (KaspiNotificationStatuses.RequiresReview, null, "invalid order code format");

        var order = await _db.PaymentOrders.FirstOrDefaultAsync(o => o.OrderCode == ev.OrderCode);
        if (order == null)
            return (KaspiNotificationStatuses.RequiresReview, null, "order not found");

        if (order.Provider != PaymentProviders.Kaspi)
            return (KaspiNotificationStatuses.RequiresReview, order.Id, "order is not a Kaspi order");

        if (!string.Equals(order.Currency, "KZT", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(ev.Currency, "KZT", StringComparison.OrdinalIgnoreCase))
            return (KaspiNotificationStatuses.RequiresReview, order.Id, "currency is not KZT");

        if (ev.Amount == null || decimal.Round(ev.Amount.Value, 2) != decimal.Round(order.Amount, 2))
            return (KaspiNotificationStatuses.RequiresReview, order.Id, "amount mismatch");

        if (order.Status != PaymentOrderStatuses.Pending)
            return (KaspiNotificationStatuses.RequiresReview, order.Id, $"order already {order.Status}");

        // Duplicate only against TRUSTED notifications (Matched/Processed); a RequiresReview/Rejected
        // record with the same payment id must never block a genuine one.
        var duplicateTrusted = await _db.KaspiPaymentNotifications.AnyAsync(x =>
            x.KaspiPaymentId == ev.KaspiPaymentId
            && (x.Status == KaspiNotificationStatuses.Matched || x.Status == KaspiNotificationStatuses.Processed));
        if (duplicateTrusted)
            return (KaspiNotificationStatuses.RequiresReview, order.Id, "payment id already matched to another notification");

        var paymentIdUsed = await _db.PaymentOrders.AnyAsync(o =>
            o.Provider == PaymentProviders.Kaspi && o.ExternalPaymentId == ev.KaspiPaymentId && o.Id != order.Id);
        if (paymentIdUsed)
            return (KaspiNotificationStatuses.RequiresReview, order.Id, "payment id already used by another order");

        return (KaspiNotificationStatuses.Matched, order.Id, null);
    }

    public async Task<IReadOnlyList<KaspiNotificationDto>> ListAsync(IEnumerable<string> statuses)
    {
        var wanted = statuses.ToList();
        var notifications = await _db.KaspiPaymentNotifications
            .Where(n => wanted.Contains(n.Status))
            .OrderByDescending(n => n.ReceivedAt)
            .ToListAsync();

        var orderIds = notifications.Where(n => n.PaymentOrderId != null).Select(n => n.PaymentOrderId!.Value).Distinct().ToList();
        var orders = orderIds.Count == 0
            ? new Dictionary<int, PaymentOrder>()
            : await _db.PaymentOrders.Include(o => o.User).Where(o => orderIds.Contains(o.Id)).ToDictionaryAsync(o => o.Id);

        var result = new List<KaspiNotificationDto>();
        foreach (var n in notifications)
        {
            PaymentOrder? order = n.PaymentOrderId != null && orders.TryGetValue(n.PaymentOrderId.Value, out var o) ? o : null;
            var amountMatches = order != null && n.Amount != null && decimal.Round(n.Amount.Value, 2) == decimal.Round(order.Amount, 2);

            var paymentIdUnique = n.KaspiPaymentId != null
                && !await _db.KaspiPaymentNotifications.AnyAsync(x =>
                    x.Id != n.Id && x.KaspiPaymentId == n.KaspiPaymentId
                    && (x.Status == KaspiNotificationStatuses.Matched || x.Status == KaspiNotificationStatuses.Processed))
                && !await _db.PaymentOrders.AnyAsync(p =>
                    p.Provider == PaymentProviders.Kaspi && p.ExternalPaymentId == n.KaspiPaymentId
                    && (order == null || p.Id != order.Id));

            result.Add(new KaspiNotificationDto(
                n.Id, n.Status, n.OrderCode, n.KaspiPaymentId, n.Amount, n.Currency, n.PaidAt, n.ReceivedAt, n.ErrorMessage,
                order?.UserId, order?.User?.Name, order?.User?.Email, order?.Amount,
                OrderFound: order != null,
                AmountMatches: amountMatches,
                PaymentIdUnique: paymentIdUnique));
        }
        return result;
    }

    public async Task<NotificationConfirmResult> ConfirmAsync(int notificationId)
    {
        var n = await _db.KaspiPaymentNotifications.FirstOrDefaultAsync(x => x.Id == notificationId);
        if (n == null) return NotificationConfirmResult.NotFound;
        if (n.Status == KaspiNotificationStatuses.Processed) return NotificationConfirmResult.AlreadyProcessed;
        // Only a Matched notification can be confirmed with one click; RequiresReview needs the manual path.
        if (n.Status != KaspiNotificationStatuses.Matched) return NotificationConfirmResult.NotMatched;
        if (n.PaymentOrderId == null || string.IsNullOrWhiteSpace(n.KaspiPaymentId))
            return NotificationConfirmResult.OrderMissing;

        var order = await _db.PaymentOrders.FirstOrDefaultAsync(o => o.Id == n.PaymentOrderId);
        if (order == null) return NotificationConfirmResult.OrderMissing;

        // Idempotent recovery: a previous confirm may have granted access and flipped the order to
        // Paid, but failed to persist Notification.Status=Processed. If the order is already Paid
        // with THIS payment id, just finish the notification — never grant again.
        if (order.Status == PaymentOrderStatuses.Paid)
        {
            if (string.Equals(order.ExternalPaymentId, n.KaspiPaymentId, StringComparison.Ordinal))
            {
                n.Status = KaspiNotificationStatuses.Processed;
                n.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return NotificationConfirmResult.AlreadyProcessed;
            }

            n.Status = KaspiNotificationStatuses.RequiresReview;
            n.ErrorMessage = "order already paid by a different payment id";
            n.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NotificationConfirmResult.DuplicatePaymentId;
        }

        // The single grant gate — atomic + idempotent.
        var result = await _orders.ConfirmAndGrantAsync(order, n.KaspiPaymentId!, amounts: null);
        switch (result)
        {
            case ConfirmResult.Granted:
            case ConfirmResult.AlreadyProcessed:
                n.Status = KaspiNotificationStatuses.Processed;
                n.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return result == ConfirmResult.Granted
                    ? NotificationConfirmResult.Granted
                    : NotificationConfirmResult.AlreadyProcessed;

            case ConfirmResult.DuplicatePaymentId:
                n.Status = KaspiNotificationStatuses.RequiresReview;
                n.ErrorMessage = "payment id already used by another order (at confirm)";
                n.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return NotificationConfirmResult.DuplicatePaymentId;

            default:
                return NotificationConfirmResult.OrderMissing;
        }
    }

    public async Task<NotificationRejectResult> RejectAsync(int notificationId)
    {
        var n = await _db.KaspiPaymentNotifications.FirstOrDefaultAsync(x => x.Id == notificationId);
        if (n == null) return NotificationRejectResult.NotFound;
        // Only an unmatched review item can be dismissed; never touch Matched/Processed or the order.
        if (n.Status != KaspiNotificationStatuses.RequiresReview) return NotificationRejectResult.NotReviewable;

        n.Status = KaspiNotificationStatuses.Rejected;
        n.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return NotificationRejectResult.Rejected;
    }

    private async Task<DateTime?> ReadCheckpointAsync()
    {
        var row = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == CheckpointKey);
        if (row == null) return null;
        return DateTime.TryParse(row.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dt)
            ? dt
            : (DateTime?)null;
    }

    private async Task WriteCheckpointAsync(DateTime value)
    {
        var row = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == CheckpointKey);
        if (row == null)
            _db.AppSettings.Add(new AppSetting { Key = CheckpointKey, Value = value.ToString("o", CultureInfo.InvariantCulture) });
        else
            row.Value = value.ToString("o", CultureInfo.InvariantCulture);
        await _db.SaveChangesAsync();
    }
}
