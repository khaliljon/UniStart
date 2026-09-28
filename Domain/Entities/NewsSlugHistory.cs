namespace UniStart.Domain.Entities;

/// <summary>
/// A previously used slug for a news article. Kept so old links keep working
/// (they resolve to the article and redirect to its current slug).
/// </summary>
public class NewsSlugHistory
{
    public int Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public int NewsArticleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual NewsArticle NewsArticle { get; set; } = null!;
}
