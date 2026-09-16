namespace UniStart.Domain.Entities;

public static class KaspiNotificationStatuses
{
    /// <summary>Email seen and stored, not yet evaluated.</summary>
    public const string Detected = "Detected";

    /// <summary>Authenticated email that strictly matches a Pending Kaspi order — ready for admin confirm.</summary>
    public const string Matched = "Matched";

    /// <summary>Something is off (auth, parsing, mismatch, duplicate) — needs manual admin review.</summary>
    public const string RequiresReview = "RequiresReview";

    /// <summary>Admin confirmed and access was granted.</summary>
    public const string Processed = "Processed";

    /// <summary>Explicitly dismissed as not a real/relevant payment.</summary>
    public const string Rejected = "Rejected";
}

public static class KaspiPaymentSources
{
    public const string Gmail = "Gmail";
    public const string KaspiApi = "KaspiApi";
}

public static class KaspiResolutionTypes
{
    public const string Refunded = "Refunded";
    public const string Ignored = "Ignored";
    public const string Other = "Other";
}

/// <summary>
/// Evidence of a Kaspi payment discovered from an external source (currently a Gmail
/// notification email). This is deliberately separate from <see cref="PaymentOrder"/> so
/// source-specific data never leaks into the payment/entitlement model, and so the source
/// can later be swapped for the official Kaspi API/webhook without schema changes.
/// Never stores raw email body, payer IIN or phone.
/// </summary>
public class KaspiPaymentNotification : IAuditable
{
    public int Id { get; set; }

    /// <summary>Evidence source, e.g. "Gmail" or "KaspiApi". See <see cref="KaspiPaymentSources"/>.</summary>
    public string Source { get; set; } = KaspiPaymentSources.Gmail;

    /// <summary>Source-native event id (Gmail message id / Kaspi callback id). Unique per source.</summary>
    public string SourceEventId { get; set; } = string.Empty;

    /// <summary>Kaspi payment id parsed from the evidence. Uniqueness is enforced at the DB level
    /// only for trusted statuses (Matched/Processed), so an unauthenticated/broken record can
    /// never block a later genuine one.</summary>
    public string? KaspiPaymentId { get; set; }

    /// <summary>Parsed order code (US-K-XXXXXXXX).</summary>
    public string? OrderCode { get; set; }

    public decimal? Amount { get; set; }

    public string Currency { get; set; } = "KZT";

    public DateTime? PaidAt { get; set; }

    /// <summary>When the source event was received (Gmail internalDate).</summary>
    public DateTime ReceivedAt { get; set; }

    /// <summary>See <see cref="KaspiNotificationStatuses"/>.</summary>
    public string Status { get; set; } = KaspiNotificationStatuses.Detected;

    /// <summary>Nullable FK to the matched order; never cascade-deletes financial history.</summary>
    public int? PaymentOrderId { get; set; }

    /// <summary>Short verdict/reason for RequiresReview/Rejected (no raw headers/body).</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>How a review item was resolved. See <see cref="KaspiResolutionTypes"/>.</summary>
    public string? ResolutionType { get; set; }
    public string? ResolutionNote { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? ResolvedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual PaymentOrder? PaymentOrder { get; set; }
}
