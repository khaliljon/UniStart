namespace UniStart.Domain.Entities;

/// <summary>
/// A CSCA news / information article. Managed from the admin panel and shown
/// on the public landing page and the student dashboard.
/// </summary>
public class NewsArticle : IAuditable
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>Short teaser shown in the card list.</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>Full article body (plain text / lightweight markdown).</summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>Kazakh title (optional; falls back to <see cref="Title"/>).</summary>
    public string? TitleKz { get; set; }
    /// <summary>English title (optional; falls back to <see cref="Title"/>).</summary>
    public string? TitleEn { get; set; }
    /// <summary>Kazakh summary (optional; falls back to <see cref="Summary"/>).</summary>
    public string? SummaryKz { get; set; }
    /// <summary>English summary (optional; falls back to <see cref="Summary"/>).</summary>
    public string? SummaryEn { get; set; }
    /// <summary>Kazakh body (optional; falls back to <see cref="Body"/>).</summary>
    public string? BodyKz { get; set; }
    /// <summary>English body (optional; falls back to <see cref="Body"/>).</summary>
    public string? BodyEn { get; set; }

    /// <summary>Optional cover image URL.</summary>
    public string? ImageUrl { get; set; }

    public bool IsPublished { get; set; }

    /// <summary>When the article was (first) published.</summary>
    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
