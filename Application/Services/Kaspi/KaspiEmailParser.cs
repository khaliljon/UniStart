using System.Globalization;
using System.Text.RegularExpressions;

namespace UniStart.Application.Services.Kaspi;

/// <summary>Fields extracted from a Kaspi payment notification email body.</summary>
public record ParsedKaspiEmail(
    string? OrderCode,
    string? KaspiPaymentId,
    decimal? Amount,
    DateTime? PaidAt,
    string? ParseError);

/// <summary>
/// Pure, side-effect-free parser for the official Kaspi notification email body. Extracts the
/// order code (from the "Название курса"/"Курстың атауы" field), the Kaspi payment id and the
/// paid amount. Never does fuzzy matching for the order code — only the exact US-K-XXXXXXXX form.
/// </summary>
public static partial class KaspiEmailParser
{
    // Order codes use an unambiguous 8-char alphabet, but we accept the strict A-Z/0-9 shape and
    // rely on an exact DB match afterwards. Never fuzzy.
    [GeneratedRegex(@"\bUS-K-[0-9A-Z]{8}\b")]
    private static partial Regex OrderCodeRegex();

    [GeneratedRegex(@"(?:Идентификатор\s+платежа|Төлем\s+идентификаторы)\s*:?\s*([0-9]{4,})", RegexOptions.IgnoreCase)]
    private static partial Regex PaymentIdRegex();

    [GeneratedRegex(@"(?:Платеж\s+на\s+сумму|Төлем\s+сомасы|сумму)\s*:?\s*([0-9][0-9\s\u00A0]*(?:[.,][0-9]{1,2})?)", RegexOptions.IgnoreCase)]
    private static partial Regex AmountRegex();

    [GeneratedRegex(@"(?:^|\n)\s*Дата\s*:?\s*([^\n\r]+)", RegexOptions.IgnoreCase)]
    private static partial Regex DateRegex();

    // The "course name" field carries the order code the user pasted into Kaspi.
    [GeneratedRegex(@"(?:Название\s+курса|Курс(?:тың)?\s+атауы)[^\n\r=:]*[=:]\s*([^\n\r]+)", RegexOptions.IgnoreCase)]
    private static partial Regex CourseFieldRegex();

    public static ParsedKaspiEmail Parse(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
            return new ParsedKaspiEmail(null, null, null, null, "Empty email body");

        var orderCode = ExtractOrderCode(body);
        var paymentId = PaymentIdRegex().Match(body) is { Success: true } pm ? pm.Groups[1].Value.Trim() : null;
        var amount = ExtractAmount(body);
        var paidAt = ExtractDate(body);

        var missing = new List<string>();
        if (orderCode == null) missing.Add("order code");
        if (paymentId == null) missing.Add("payment id");
        if (amount == null) missing.Add("amount");

        var error = missing.Count > 0 ? $"Missing required field(s): {string.Join(", ", missing)}" : null;
        return new ParsedKaspiEmail(orderCode, paymentId, amount, paidAt, error);
    }

    private static string? ExtractOrderCode(string body)
    {
        // Prefer the value inside the course-name field; fall back to the whole body.
        var field = CourseFieldRegex().Match(body);
        if (field.Success)
        {
            var inField = OrderCodeRegex().Match(field.Groups[1].Value);
            if (inField.Success) return inField.Value;
        }
        var anywhere = OrderCodeRegex().Match(body);
        return anywhere.Success ? anywhere.Value : null;
    }

    private static decimal? ExtractAmount(string body)
    {
        var m = AmountRegex().Match(body);
        if (!m.Success) return null;
        var raw = m.Groups[1].Value.Replace("\u00A0", "").Replace(" ", "").Replace(',', '.');
        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : null;
    }

    private static DateTime? ExtractDate(string body)
    {
        var m = DateRegex().Match(body);
        if (!m.Success) return null;
        var raw = m.Groups[1].Value.Trim();
        return DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt)
            ? dt
            : null;
    }
}
