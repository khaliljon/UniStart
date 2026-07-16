using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

/// <summary>
/// Polar (Stripe-backed) payments. Prices live in our DB; we send an ad-hoc fixed
/// price to Polar per checkout and carry a compact line encoding in metadata so the
/// webhook can grant the right entitlements. Webhooks are verified with the
/// Standard Webhooks HMAC scheme.
/// </summary>
public class PolarService : IPolarService
{
    private readonly IEntitlementService _entitlements;
    private readonly IEmailService _email;
    private readonly UniStartDbContext _db;
    private readonly ILogger<PolarService> _logger;
    private readonly HttpClient _http;

    private readonly string _token;
    private readonly string _webhookSecret;
    private readonly string _productId;
    private readonly string _successUrl;
    private readonly string _baseUrl;

    public PolarService(IConfiguration config, IEntitlementService entitlements, IEmailService email, UniStartDbContext db, ILogger<PolarService> logger)
    {
        _entitlements = entitlements;
        _email = email;
        _db = db;
        _logger = logger;

        _token = config["POLAR_SANDBOX_ACCESS_TOKEN"] ?? config["POLAR_ACCESS_TOKEN"] ?? config["Polar:AccessToken"] ?? "";
        _webhookSecret = config["POLAR_SANDBOX_WEBHOOK_SECRET"] ?? config["POLAR_WEBHOOK_SECRET"] ?? config["Polar:WebhookSecret"] ?? "";
        _productId = config["POLAR_PRODUCT_ID"] ?? config["Polar:ProductId"] ?? "";
        _successUrl = config["POLAR_SUCCESS_URL"] ?? config["Polar:SuccessUrl"] ?? "https://unistart.kz/purchases?paid=1";
        _baseUrl = (config["POLAR_API_BASE"] ?? config["Polar:ApiBase"] ?? "https://sandbox-api.polar.sh").TrimEnd('/');

        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public async Task<string> CreateCheckoutUrlAsync(int userId, List<CheckoutLineDto> lines)
    {
        if (string.IsNullOrEmpty(_token) || string.IsNullOrEmpty(_productId))
            throw new InvalidOperationException("Polar is not configured (token/product id missing).");

        // Server-side price — never trust the client.
        var quote = await _entitlements.QuoteAsync(lines);
        var currency = quote.Currency.ToLowerInvariant();
        // Zero-decimal currencies (KZT, JPY, KRW, …) are sent as-is; others in cents.
        var amountMinor = IsZeroDecimal(currency)
            ? (long)Math.Round(quote.Total)
            : (long)Math.Round(quote.Total * 100m);

        var payload = new
        {
            products = new[] { _productId },
            prices = new Dictionary<string, object[]>
            {
                [_productId] = new object[]
                {
                    new { amount_type = "fixed", price_amount = amountMinor, price_currency = currency }
                }
            },
            currency,
            success_url = _successUrl,
            external_customer_id = userId.ToString(),
            metadata = new Dictionary<string, string>
            {
                ["userId"] = userId.ToString(),
                ["lines"] = EncodeLines(lines),
            },
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/checkouts/");
        req.Headers.Add("Authorization", $"Bearer {_token}");
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var resp = await _http.SendAsync(req);
        var body = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("Polar checkout failed: {Status} {Body}", resp.StatusCode, body);
            throw new InvalidOperationException("Не удалось создать оплату Polar.");
        }

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("url").GetString()
               ?? throw new InvalidOperationException("Polar did not return a checkout url.");
    }

    public async Task<bool> HandleWebhookAsync(string rawBody, string? webhookId, string? webhookTimestamp, string? webhookSignature)
    {
        if (!VerifySignature(rawBody, webhookId, webhookTimestamp, webhookSignature))
        {
            _logger.LogWarning("Polar webhook signature verification failed");
            return false;
        }

        using var doc = JsonDocument.Parse(rawBody);
        var root = doc.RootElement;
        var type = root.TryGetProperty("type", out var t) ? t.GetString() : null;

        // Grant on a paid order (Polar sends the paid transition as order.updated).
        if (type != "order.paid" && type != "order.updated" && type != "order.created" && type != "checkout.updated")
            return true; // acknowledged, nothing to do

        if (!root.TryGetProperty("data", out var data)) return true;

        // Only act once payment actually succeeded.
        if (type == "checkout.updated")
        {
            var status = data.TryGetProperty("status", out var st) ? st.GetString() : null;
            if (status != "succeeded") return true;
        }
        else // order.*
        {
            var paidFlag = data.TryGetProperty("paid", out var pd) && pd.ValueKind == JsonValueKind.True;
            var orderStatus = data.TryGetProperty("status", out var os) ? os.GetString() : null;
            if (!paidFlag && orderStatus != "paid") return true;
        }

        var orderId = data.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        var metadata = ExtractMetadata(data);
        if (metadata == null || !metadata.TryGetValue("userId", out var userIdRaw)
            || !int.TryParse(userIdRaw, out var userId)
            || !metadata.TryGetValue("lines", out var linesRaw))
        {
            _logger.LogWarning("Polar webhook missing metadata; type={Type}", type);
            return true;
        }

        // Idempotency: skip if we already granted for this order/checkout.
        var dedupKey = $"polar:{orderId ?? webhookId}";
        if (await _db.AppSettings.AnyAsync(s => s.Key == dedupKey)) return true;

        var lines = DecodeLines(linesRaw);
        if (lines.Count == 0) return true;

        await _entitlements.GrantAsync(userId, lines);
        _db.AppSettings.Add(new AppSetting { Key = dedupKey, Value = DateTime.UtcNow.ToString("o") });
        await _db.SaveChangesAsync();

        _logger.LogInformation("Polar grant applied for user {UserId}, order {OrderId}", userId, orderId);

        // Best-effort purchase receipt (never fail the webhook on email errors).
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                var quote = await _entitlements.QuoteAsync(lines);
                var name = string.IsNullOrWhiteSpace(user.Name) ? user.FirstName : user.Name;
                await _email.SendPurchaseReceiptAsync(user.Email, name, quote.Total, quote.Currency);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send purchase receipt for user {UserId}", userId);
        }

        return true;
    }

    // Zero-decimal currencies charged as whole units (Stripe's canonical list).
    // NOTE: KZT is a 2-decimal currency in Stripe/Polar, so it is NOT here — it uses ×100.
    private static readonly HashSet<string> ZeroDecimal = new(StringComparer.OrdinalIgnoreCase)
    {
        "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga",
        "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf",
    };

    private static bool IsZeroDecimal(string currency) => ZeroDecimal.Contains(currency);

    // ── Standard Webhooks signature ────────────────────────

    private bool VerifySignature(string body, string? id, string? timestamp, string? signatureHeader)
    {
        if (string.IsNullOrEmpty(_webhookSecret))
        {
            _logger.LogWarning("Polar webhook: signing secret is not configured (POLAR_SANDBOX_WEBHOOK_SECRET)");
            return false;
        }
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signatureHeader))
        {
            _logger.LogWarning("Polar webhook: missing webhook-id/timestamp/signature headers");
            return false;
        }

        // Standard Webhooks: secret is "whsec_<base64>"; the HMAC key is the
        // base64-decoded remainder. Try both the decoded key and the raw bytes
        // to be robust against secret-format differences.
        var secretPart = _webhookSecret.StartsWith("whsec_") ? _webhookSecret["whsec_".Length..] : _webhookSecret;
        var keys = new List<byte[]>();
        keys.Add(Encoding.UTF8.GetBytes(_webhookSecret));
        var decoded = DecodeBase64(secretPart);
        if (decoded != null) keys.Add(decoded);
        keys.Add(Encoding.UTF8.GetBytes(secretPart));

        var signedContent = $"{id}.{timestamp}.{body}";
        var received = signatureHeader
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Contains(',') ? p.Split(',', 2)[1] : p)
            .ToList();

        foreach (var key in keys)
        {
            using var hmac = new HMACSHA256(key);
            var expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedContent));
            foreach (var sigB64 in received)
            {
                var sigBytes = DecodeBase64(sigB64);
                if (sigBytes != null && CryptographicOperations.FixedTimeEquals(sigBytes, expected))
                    return true;
            }
        }

        // TEMP DEBUG — remove after diagnosis. Logs byte-level details (never the secret itself).
        using (var dbg = new HMACSHA256(keys[0]))
        {
            var exp = Convert.ToBase64String(dbg.ComputeHash(Encoding.UTF8.GetBytes(signedContent)));
            _logger.LogWarning(
                "Polar webhook signature mismatch. id={Id} ts={Ts} bodyLen={Len} keyLen={KeyLen} expectedSig={Exp} receivedHeader={Recv}",
                id, timestamp, body.Length, keys[0].Length, exp, signatureHeader);
        }
        return false;
    }

    /// <summary>Base64 decode tolerant of missing padding and url-safe alphabet.</summary>
    private static byte[]? DecodeBase64(string s)
    {
        s = s.Trim().Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
            case 1: return null;
        }
        try { return Convert.FromBase64String(s); }
        catch { return null; }
    }

    private static Dictionary<string, string>? ExtractMetadata(JsonElement data)
    {
        if (!data.TryGetProperty("metadata", out var meta) || meta.ValueKind != JsonValueKind.Object)
            return null;
        var dict = new Dictionary<string, string>();
        foreach (var p in meta.EnumerateObject())
            dict[p.Name] = p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() ?? "" : p.Value.ToString();
        return dict;
    }

    // ── Compact line encoding for metadata ─────────────────
    //  mock:    "m:<mockId>:<runs>"
    //  package: "p:<key>:<id,id,...>"
    //  joined with ";"

    private static string EncodeLines(List<CheckoutLineDto> lines)
    {
        var parts = new List<string>();
        foreach (var l in lines)
        {
            if (l.Kind == "mock")
                parts.Add($"m:{l.MockExamId}:{l.Runs}");
            else if (l.Kind == "package")
                parts.Add($"p:{l.PackageKey}:{string.Join(",", l.SelectedMockIds ?? new List<int>())}");
            else if (l.Kind == "book")
                parts.Add($"b:{l.BookMaterialId}");
        }
        return string.Join(";", parts);
    }

    private static List<CheckoutLineDto> DecodeLines(string encoded)
    {
        var result = new List<CheckoutLineDto>();
        foreach (var token in encoded.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var seg = token.Split(':');
            if (seg.Length < 2) continue;
            if (seg[0] == "m" && seg.Length == 3
                && int.TryParse(seg[1], out var mockId) && int.TryParse(seg[2], out var runs))
            {
                result.Add(new CheckoutLineDto { Kind = "mock", MockExamId = mockId, Runs = runs });
            }
            else if (seg[0] == "p" && seg.Length >= 2)
            {
                var ids = seg.Length >= 3 && seg[2].Length > 0
                    ? seg[2].Split(',').Where(x => int.TryParse(x, out _)).Select(int.Parse).ToList()
                    : new List<int>();
                result.Add(new CheckoutLineDto { Kind = "package", PackageKey = seg[1], SelectedMockIds = ids });
            }
            else if (seg[0] == "b" && seg.Length >= 2 && int.TryParse(seg[1], out var bookId))
            {
                result.Add(new CheckoutLineDto { Kind = "book", BookMaterialId = bookId });
            }
        }
        return result;
    }
}
