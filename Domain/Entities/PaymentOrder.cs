using System.Security.Cryptography;

namespace UniStart.Domain.Entities;

public static class PaymentProviders
{
    public const string Polar = "Polar";
    public const string Kaspi = "Kaspi";
}

public static class PaymentOrderStatuses
{
    public const string Pending = "Pending";
    public const string Paid = "Paid";
    public const string Cancelled = "Cancelled";
}

/// <summary>
/// Provider-neutral checkout order. Created as Pending before any redirect, holds the
/// trusted server-resolved lines/price, and is flipped to Paid exactly once when the
/// payment is confirmed (Polar webhook or Kaspi admin/callback).
/// </summary>
public class PaymentOrder : IAuditable
{
    public int Id { get; set; }

    /// <summary>Public, unpredictable, human-copyable code, e.g. US-K-7QF2M9AH.</summary>
    public string OrderCode { get; set; } = string.Empty;

    public int UserId { get; set; }

    /// <summary>See <see cref="PaymentProviders"/>.</summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>See <see cref="PaymentOrderStatuses"/>.</summary>
    public string Status { get; set; } = PaymentOrderStatuses.Pending;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "KZT";

    /// <summary>JSON-serialized List&lt;CheckoutLineDto&gt; (jsonb).</summary>
    public string LinesJson { get; set; } = "[]";

    /// <summary>Polar order id / Kaspi payment id. Unique per provider once set.</summary>
    public string? ExternalPaymentId { get; set; }

    /// <summary>Polar success-page correlation id (cref).</summary>
    public string? CheckoutRef { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    // Crockford-style alphabet without ambiguous chars (0/O, 1/I/L).
    private const string CodeAlphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";

    public static string ProviderLetter(string provider) => provider switch
    {
        PaymentProviders.Kaspi => "K",
        PaymentProviders.Polar => "P",
        _ => "X",
    };

    /// <summary>Generates a public order code like US-K-7QF2M9AH (8 random chars).</summary>
    public static string GenerateOrderCode(string provider, int length = 8)
    {
        var chars = new char[length];
        for (var i = 0; i < length; i++)
            chars[i] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];
        return $"US-{ProviderLetter(provider)}-{new string(chars)}";
    }
}
