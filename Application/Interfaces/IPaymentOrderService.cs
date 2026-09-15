using UniStart.Application.DTOs;
using UniStart.Domain.Entities;

namespace UniStart.Application.Interfaces;

public enum ConfirmResult
{
    /// <summary>Access was granted and the order flipped to Paid by this call.</summary>
    Granted,

    /// <summary>The order was already Paid (double webhook / double confirm) — no-op.</summary>
    AlreadyProcessed,

    /// <summary>The external payment id is already tied to a different order.</summary>
    DuplicatePaymentId,
}

public interface IPaymentOrderService
{
    /// <summary>
    /// Creates a Pending order with an immutable server-resolved snapshot (trusted price and
    /// exact grants), so the provider can never influence what was bought or for how much.
    /// When <paramref name="requiredCurrency"/> is set, the currency is validated before the
    /// order is persisted (no stray Pending row on mismatch).
    /// </summary>
    Task<PaymentOrder> CreateAsync(int userId, string provider, List<CheckoutLineDto> lines, string? checkoutRef = null, string? requiredCurrency = null);

    /// <summary>Deserializes the immutable order snapshot stored on the order.</summary>
    OrderSnapshot DeserializeSnapshot(PaymentOrder order);

    /// <summary>
    /// Atomically flips the order Pending → Paid and grants access exactly once.
    /// Safe against concurrent/duplicate calls. Used by both the Polar webhook and the
    /// Kaspi manual/automatic confirmation.
    /// </summary>
    Task<ConfirmResult> ConfirmAndGrantAsync(PaymentOrder order, string externalPaymentId, PurchaseAmountsDto? amounts = null);

    /// <summary>
    /// Sends the existing "purchase receipt" email for an order (provider-neutral). Best-effort:
    /// never throws — an email failure must not affect the payment/grant. Call only after a real
    /// first-time grant (result == Granted).
    /// </summary>
    Task SendPurchaseReceiptForOrderAsync(PaymentOrder order);
}
