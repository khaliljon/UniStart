namespace UniStart.Application.Interfaces;

/// <summary>Provides currency exchange rates for money reporting (e.g. Polar commission conversion).</summary>
public interface IExchangeRateService
{
    /// <summary>
    /// Current USD→KZT rate. Cached and refreshed automatically from an external API;
    /// falls back to the configured <c>Polar:UsdToLocalRate</c> when the API is unavailable.
    /// </summary>
    Task<decimal> GetUsdToKztAsync(CancellationToken ct = default);
}
