namespace UniStart.Domain.Entities;

public class Purchase : IAuditable
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ItemType { get; set; } = string.Empty;

    public string ItemCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Subjects { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "KZT";

    public decimal GrossAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PlatformFeeAmount { get; set; }
    public string? PlatformFeeCurrency { get; set; }
    public decimal NetAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public string? PolarOrderId { get; set; }
    public string? CheckoutRef { get; set; }

    // Provider-neutral payment linkage (works for Polar, Kaspi, and future providers).
    public string? PaymentProvider { get; set; }
    public string? ExternalPaymentId { get; set; }
    public int? PaymentOrderId { get; set; }

    public string Status { get; set; } = "Paid";

    public DateTime PurchasedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
