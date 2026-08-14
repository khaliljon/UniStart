using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UniStart.Application.Interfaces;

namespace UniStart.Application.Services;

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

        var fetched = await FetchAsync(ct);
        if (fetched is > 0)
        {
            _cache.Set(CacheKey, fetched.Value, TimeSpan.FromHours(12));
            return fetched.Value;
        }

        _cache.Set(CacheKey, fallback, TimeSpan.FromMinutes(30));
        return fallback;
    }

    public async Task RefreshAsync()
    {
        var fetched = await FetchAsync(CancellationToken.None);
        if (fetched is > 0)
        {
            _cache.Set(CacheKey, fetched.Value, TimeSpan.FromHours(24));
            _logger.LogInformation("Refreshed USD/KZT rate: {Rate}", fetched.Value);
        }
    }

    private async Task<decimal?> FetchAsync(CancellationToken ct)
    {
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
                return rate;
            }
            _logger.LogWarning("USD/KZT rate missing in exchange API response");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch USD/KZT rate");
        }
        return null;
    }
}
