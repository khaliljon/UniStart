using System.Text.RegularExpressions;

namespace UniStart.Application.Helpers;

public static partial class InputSanitizer
{
    [GeneratedRegex("<[^>]*>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    public static string? Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return HtmlTagRegex().Replace(input, string.Empty).Trim();
    }
}
