using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

/// <summary>
/// Prices, validates and grants run-based mock entitlements. Shared by the
/// checkout endpoint (stub) and the Polar webhook so grants are computed the
/// same way in both flows. Prices are always recomputed server-side.
/// </summary>
public interface IEntitlementService
{
    /// <summary>Public catalog for the storefront, including the user's balances.</summary>
    Task<MockCatalogDto> GetCatalogAsync(int userId);

    /// <summary>Validate lines against the catalog and compute the total. Throws on invalid lines.</summary>
    Task<CheckoutQuoteDto> QuoteAsync(List<CheckoutLineDto> lines);

    /// <summary>Grant the runs described by the lines to the user and record purchases.</summary>
    Task<CheckoutQuoteDto> GrantAsync(int userId, List<CheckoutLineDto> lines, PurchaseAmountsDto? amounts = null);
}
