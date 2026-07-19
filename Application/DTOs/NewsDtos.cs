using System.ComponentModel.DataAnnotations;

namespace UniStart.Application.DTOs;

/// <summary>News article payload returned to clients.</summary>
public record NewsArticleDto(
    int Id,
    string Title,
    string Summary,
    string Body,
    string? ImageUrl,
    bool IsPublished,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? TitleKz = null,
    string? TitleEn = null,
    string? SummaryKz = null,
    string? SummaryEn = null,
    string? BodyKz = null,
    string? BodyEn = null);

/// <summary>Admin create/update payload for a news article.</summary>
public class NewsUpsertDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Summary { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsPublished { get; set; }

    [MaxLength(200)]
    public string? TitleKz { get; set; }

    [MaxLength(200)]
    public string? TitleEn { get; set; }

    [MaxLength(500)]
    public string? SummaryKz { get; set; }

    [MaxLength(500)]
    public string? SummaryEn { get; set; }

    public string? BodyKz { get; set; }

    public string? BodyEn { get; set; }
}
