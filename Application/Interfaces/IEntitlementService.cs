using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IEntitlementService
{
    Task<MockCatalogDto> GetCatalogAsync(int userId);

    Task<CheckoutQuoteDto> QuoteAsync(List<CheckoutLineDto> lines);

    Task<CheckoutQuoteDto> GrantAsync(int userId, List<CheckoutLineDto> lines, PurchaseAmountsDto? amounts = null, string? polarOrderId = null, string? checkoutRef = null);

    Task UpdatePurchaseAmountsAsync(string polarOrderId, PurchaseAmountsDto amounts);
}
