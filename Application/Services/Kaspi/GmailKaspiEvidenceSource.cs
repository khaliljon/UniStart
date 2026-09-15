using System.Text;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;

namespace UniStart.Application.Services.Kaspi;

/// <summary>
/// Reads potential Kaspi payment notification emails from a dedicated Gmail mailbox using a
/// read-only OAuth refresh token. Only searches for messages from the configured sender (and
/// optional label); never sends/modifies/deletes. Produces provider-neutral evidence — no raw
/// body, IIN or phone is retained.
/// </summary>
public class GmailKaspiEvidenceSource : IPaymentEvidenceSource
{
    private readonly ILogger<GmailKaspiEvidenceSource> _logger;

    private readonly bool _enabled;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _refreshToken;
    private readonly string _user;
    private readonly string _sender;
    private readonly string? _label;
    private readonly int _lookbackDays;

    private GmailService? _service;

    public GmailKaspiEvidenceSource(IConfiguration config, ILogger<GmailKaspiEvidenceSource> logger)
    {
        _logger = logger;
        _enabled = config.GetValue<bool>("GMAIL_KASPI_ENABLED");
        _clientId = config["GMAIL_KASPI_CLIENT_ID"] ?? "";
        _clientSecret = config["GMAIL_KASPI_CLIENT_SECRET"] ?? "";
        _refreshToken = config["GMAIL_KASPI_REFRESH_TOKEN"] ?? "";
        _user = string.IsNullOrWhiteSpace(config["GMAIL_KASPI_USER"]) ? "me" : config["GMAIL_KASPI_USER"]!;
        _sender = string.IsNullOrWhiteSpace(config["GMAIL_KASPI_FROM"]) ? "kaspi.payments@kaspibank.kz" : config["GMAIL_KASPI_FROM"]!;
        _label = config["GMAIL_KASPI_LABEL"];
        _lookbackDays = config.GetValue<int?>("GMAIL_KASPI_LOOKBACK_DAYS") ?? 7;
    }

    public bool Enabled =>
        _enabled && _clientId.Length > 0 && _clientSecret.Length > 0 && _refreshToken.Length > 0;

    public async Task<IReadOnlyList<KaspiPaymentEvidence>> FetchNewAsync(DateTime? sinceUtc, CancellationToken ct = default)
    {
        if (!Enabled) return Array.Empty<KaspiPaymentEvidence>();

        var service = BuildService();
        var query = BuildQuery(sinceUtc);

        var evidence = new List<KaspiPaymentEvidence>();
        string? pageToken = null;
        do
        {
            var listReq = service.Users.Messages.List(_user);
            listReq.Q = query;
            listReq.MaxResults = 50;
            listReq.PageToken = pageToken;

            var resp = await listReq.ExecuteAsync(ct);
            if (resp.Messages != null)
            {
                foreach (var msgRef in resp.Messages)
                {
                    var full = await service.Users.Messages.Get(_user, msgRef.Id).ExecuteAsync(ct);
                    evidence.Add(BuildEvidence(full));
                }
            }
            pageToken = resp.NextPageToken;
        }
        while (!string.IsNullOrEmpty(pageToken) && evidence.Count < 200);

        return evidence;
    }

    private GmailService BuildService()
    {
        if (_service != null) return _service;

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets { ClientId = _clientId, ClientSecret = _clientSecret },
            Scopes = new[] { GmailService.Scope.GmailReadonly },
        });
        var credential = new UserCredential(flow, _user, new TokenResponse { RefreshToken = _refreshToken });

        _service = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "UniStart",
        });
        return _service;
    }

    private string BuildQuery(DateTime? sinceUtc)
    {
        var after = sinceUtc ?? DateTime.UtcNow.AddDays(-_lookbackDays);
        var epochSeconds = new DateTimeOffset(after, TimeSpan.Zero).ToUnixTimeSeconds();
        var sb = new StringBuilder($"from:{_sender} after:{epochSeconds}");
        if (!string.IsNullOrWhiteSpace(_label))
            sb.Append($" label:{_label}");
        return sb.ToString();
    }

    private KaspiPaymentEvidence BuildEvidence(Message message)
    {
        var headers = message.Payload?.Headers ?? new List<MessagePartHeader>();
        string? Header(string name) => headers.FirstOrDefault(h => string.Equals(h.Name, name, StringComparison.OrdinalIgnoreCase))?.Value;

        var body = ExtractPlainText(message.Payload);
        var parsed = KaspiEmailParser.Parse(body);

        var (auth, authReason) = EvaluateAuth(Header("From"), Header("Authentication-Results"));

        var receivedAt = message.InternalDate.HasValue
            ? DateTimeOffset.FromUnixTimeMilliseconds(message.InternalDate.Value).UtcDateTime
            : DateTime.UtcNow;

        return new KaspiPaymentEvidence(
            Source: KaspiPaymentSources.Gmail,
            SourceMessageId: message.Id,
            OrderCode: parsed.OrderCode,
            KaspiPaymentId: parsed.KaspiPaymentId,
            Amount: parsed.Amount,
            Currency: "KZT",
            PaidAt: parsed.PaidAt,
            ReceivedAt: receivedAt,
            Auth: auth,
            AuthReason: authReason,
            ParseError: parsed.ParseError);
    }

    // Stage 1: for an automatic Matched we require the exact configured sender AND DMARC=pass.
    // SPF/DKIM are kept only as diagnostics; SPF/DKIM pass without DMARC pass is NOT enough.
    // We intentionally do NOT hardcode a DKIM d= domain yet; once real Kaspi headers are confirmed,
    // domain alignment can be tightened here in one place. Only a short verdict/reason is kept.
    private (EmailAuthVerdict Verdict, string? Reason) EvaluateAuth(string? fromHeader, string? authResults)
    {
        var fromAddr = ExtractAddress(fromHeader);
        if (fromAddr == null || !string.Equals(fromAddr, _sender, StringComparison.OrdinalIgnoreCase))
            return (EmailAuthVerdict.Fail, $"unexpected sender: {fromAddr ?? "none"}");

        if (string.IsNullOrWhiteSpace(authResults))
            return (EmailAuthVerdict.Unknown, "no Authentication-Results");

        var lower = authResults.ToLowerInvariant();
        var dmarcPass = Regex.IsMatch(lower, @"dmarc\s*=\s*pass");
        var dmarcFail = Regex.IsMatch(lower, @"dmarc\s*=\s*(fail|softfail|permerror|temperror|bestguesspass|none)");

        // Diagnostics only — never sufficient on their own.
        var spf = Regex.Match(lower, @"spf\s*=\s*(\w+)").Groups[1].Value;
        var dkim = Regex.Match(lower, @"dkim\s*=\s*(\w+)").Groups[1].Value;
        var diag = $"spf={(string.IsNullOrEmpty(spf) ? "?" : spf)};dkim={(string.IsNullOrEmpty(dkim) ? "?" : dkim)}";

        if (dmarcPass) return (EmailAuthVerdict.Pass, $"dmarc=pass;{diag}");
        if (dmarcFail) return (EmailAuthVerdict.Fail, $"dmarc=fail;{diag}");
        return (EmailAuthVerdict.Unknown, $"dmarc=missing;{diag}");
    }

    private static string? ExtractAddress(string? headerValue)
    {
        if (string.IsNullOrWhiteSpace(headerValue)) return null;
        var m = Regex.Match(headerValue, @"[\w.+-]+@[\w.-]+\.[A-Za-z]{2,}");
        return m.Success ? m.Value.Trim() : null;
    }

    private static string? ExtractPlainText(MessagePart? part)
    {
        if (part == null) return null;

        if (string.Equals(part.MimeType, "text/plain", StringComparison.OrdinalIgnoreCase) && part.Body?.Data != null)
            return DecodeBase64Url(part.Body.Data);

        if (part.Parts != null)
        {
            foreach (var child in part.Parts)
            {
                var text = ExtractPlainText(child);
                if (!string.IsNullOrWhiteSpace(text)) return text;
            }
        }

        // Fall back to a single-part HTML body with tags stripped.
        if (string.Equals(part.MimeType, "text/html", StringComparison.OrdinalIgnoreCase) && part.Body?.Data != null)
            return Regex.Replace(DecodeBase64Url(part.Body.Data), "<[^>]+>", " ");

        return null;
    }

    private static string DecodeBase64Url(string data)
    {
        var s = data.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; }
        try { return Encoding.UTF8.GetString(Convert.FromBase64String(s)); }
        catch { return string.Empty; }
    }
}
