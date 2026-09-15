using UniStart.Application.DTOs;
using UniStart.Domain.Entities;

namespace UniStart.Application.Interfaces;

public interface IEntitlementService
{
    Task<MockCatalogDto> GetCatalogAsync(int userId);

    Task<CheckoutQuoteDto> QuoteAsync(List<CheckoutLineDto> lines);

    /// <summary>Resolves an immutable server snapshot (prices, titles, grants) from raw lines.</summary>
    Task<OrderSnapshot> ResolveSnapshotAsync(List<CheckoutLineDto> lines);

    Task<CheckoutQuoteDto> GrantAsync(int userId, List<CheckoutLineDto> lines, PurchaseAmountsDto? amounts = null, string? externalPaymentId = null, string? checkoutRef = null, PaymentOrder? order = null);

    /// <summary>Grants access strictly from a previously stored snapshot (no live catalog access).</summary>
    Task GrantFromSnapshotAsync(int userId, OrderSnapshot snapshot, PurchaseAmountsDto? amounts = null, string? externalPaymentId = null, string? checkoutRef = null, PaymentOrder? order = null);

    Task UpdatePurchaseAmountsAsync(string polarOrderId, PurchaseAmountsDto amounts);
}
