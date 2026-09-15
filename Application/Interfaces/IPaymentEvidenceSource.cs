namespace UniStart.Application.Interfaces;

/// <summary>Result of checking a message's email authentication (SPF/DKIM/DMARC).</summary>
public enum EmailAuthVerdict
{
    /// <summary>No Authentication-Results present or could not be evaluated.</summary>
    Unknown = 0,

    /// <summary>Authentication explicitly passed.</summary>
    Pass = 1,

    /// <summary>Authentication explicitly failed.</summary>
    Fail = 2,
}

/// <summary>
/// Provider-neutral evidence that a Kaspi payment happened. Today it is produced from a Gmail
/// notification email; later it can be produced from the official Kaspi API/webhook without
/// touching the matching/confirm logic.
/// </summary>
public record KaspiPaymentEvidence(
    string Source,
    string SourceMessageId,
    string? OrderCode,
    string? KaspiPaymentId,
    decimal? Amount,
    string Currency,
    DateTime? PaidAt,
    DateTime ReceivedAt,
    EmailAuthVerdict Auth,
    string? AuthReason,
    string? ParseError);

/// <summary>
/// A replaceable source of Kaspi payment evidence. The Gmail adapter is the first implementation;
/// a Kaspi API/webhook adapter can replace it later with no changes to matching/confirmation.
/// </summary>
public interface IPaymentEvidenceSource
{
    bool Enabled { get; }

    /// <summary>
    /// Fetches evidence messages received at/after <paramref name="sinceUtc"/>. Intended to be
    /// at-least-once: the caller deduplicates by <see cref="KaspiPaymentEvidence.SourceMessageId"/>,
    /// so a small overlap window is safe and expected.
    /// </summary>
    Task<IReadOnlyList<KaspiPaymentEvidence>> FetchNewAsync(DateTime? sinceUtc, CancellationToken ct = default);
}
