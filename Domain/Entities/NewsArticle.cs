namespace UniStart.Domain.Entities;

/// <summary>
/// A CSCA news / information article. Managed from the admin panel and shown
/// on the public landing page and the student dashboard.
/// </summary>
public class NewsArticle : IAuditable
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

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
