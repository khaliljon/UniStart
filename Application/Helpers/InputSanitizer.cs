using System.Text.RegularExpressions;

namespace UniStart.Application.Helpers;

/// <summary>
/// Strips HTML tags from user-provided text to prevent stored XSS.
/// Applied to user-facing fields (names, bios, messages) at the service boundary.
/// Admin/question content is intentionally NOT sanitized (may contain KaTeX/markdown).
/// </summary>
public static partial class InputSanitizer
{
    [GeneratedRegex("<[^>]*>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    /// <summary>
    /// Strips HTML tags and trims whitespace. Returns null if input is null.
    /// </summary>
    public static string? Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return HtmlTagRegex().Replace(input, string.Empty).Trim();
    }
}
