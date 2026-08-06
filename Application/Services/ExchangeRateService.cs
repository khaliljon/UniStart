using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UniStart.Application.Interfaces;

namespace UniStart.Application.Services;

/// <summary>
/// Fetches the USD→KZT exchange rate from a free no-key API (open.er-api.com),
/// caches it for 12h, and falls back to the configured rate when the API is down.
/// </summary>
public class ExchangeRateService : IExchangeRateService
{
    private const string CacheKey = "fx:usd_kzt";
    private const string ApiUrl = "https://open.er-api.com/v6/latest/USD";
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(10) };

    private readonly IMemoryCache _cache;
    private readonly IConfiguration _config;
    private readonly ILogger<ExchangeRateService> _logger;

    public ExchangeRateService(IMemoryCache cache, IConfiguration config, ILogger<ExchangeRateService> logger)
    {
        _cache = cache;
        _config = config;
        _logger = logger;
    }

    public async Task<decimal> GetUsdToKztAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out decimal cached))
            return cached;

        var fallback = _config.GetValue<decimal?>("Polar:UsdToLocalRate") ?? 500m;

        try
        {
            using var resp = await _http.GetAsync(ApiUrl, ct);
            resp.EnsureSuccessStatusCode();
            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

            if (doc.RootElement.TryGetProperty("rates", out var rates)
                && rates.TryGetProperty("KZT", out var kzt)
                && kzt.TryGetDecimal(out var rate) && rate > 0)
            {
                _cache.Set(CacheKey, rate, TimeSpan.FromHours(12));
                return rate;
            }
            _logger.LogWarning("USD/KZT rate missing in exchange API response; using fallback {Fallback}", fallback);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch USD/KZT rate; using fallback {Fallback}", fallback);
        }

        // Cache the fallback briefly so a downed API isn't hit on every request.
        _cache.Set(CacheKey, fallback, TimeSpan.FromMinutes(30));
        return fallback;
    }
}
