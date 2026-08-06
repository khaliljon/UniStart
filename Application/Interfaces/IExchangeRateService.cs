namespace UniStart.Application.Interfaces;

/// <summary>Provides currency exchange rates for money reporting (e.g. Polar commission conversion).</summary>
public interface IExchangeRateService
{
    /// <summary>
    /// Current USD→KZT rate. Returns the cached value; if the cache is empty it fetches
    /// from the external API, falling back to the configured <c>Polar:UsdToLocalRate</c>.
    /// </summary>
    Task<decimal> GetUsdToKztAsync(CancellationToken ct = default);

    /// <summary>
    /// Forces a fresh fetch of the USD→KZT rate and updates the cache. Invoked by the
    /// daily background job so requests always hit a warm cache.
    /// </summary>
    Task RefreshAsync();
}
