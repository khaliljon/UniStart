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
    DateTime? UpdatedAt);

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
}
