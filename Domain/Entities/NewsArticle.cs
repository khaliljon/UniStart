namespace UniStart.Domain.Entities;

public static class NewsCategories
{
    public const string Dates = "dates";
    public const string Admission = "admission";
    public const string Platform = "platform";
    public const string Guide = "guide";

    public static readonly string[] All = { Dates, Admission, Platform, Guide };

    public static string Normalize(string? value) =>
        value != null && All.Contains(value) ? value : Admission;
}

/// <summary>
/// A CSCA news / information article. Managed from the admin panel and shown
/// on the public landing page and the student dashboard.
/// </summary>
public class NewsArticle : IAuditable
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>URL-friendly identifier used by /csca/news/{slug}.</summary>
    public string? Slug { get; set; }

    /// <summary>See <see cref="NewsCategories"/>.</summary>
    public string Category { get; set; } = NewsCategories.Admission;

    /// <summary>Highlighted as the lead story on the news portal.</summary>
    public bool IsFeatured { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string? TitleKz { get; set; }
    public string? TitleEn { get; set; }
    public string? SummaryKz { get; set; }
    public string? SummaryEn { get; set; }
    public string? BodyKz { get; set; }
    public string? BodyEn { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
