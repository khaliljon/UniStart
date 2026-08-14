namespace UniStart.Domain.Entities;

public class LegalDocument : IAuditable
{
    public int Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string LastUpdatedLabel { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? TitleKz { get; set; }
    public string? TitleEn { get; set; }
    public string? ContentKz { get; set; }
    public string? ContentEn { get; set; }
    public string? LastUpdatedLabelKz { get; set; }
    public string? LastUpdatedLabelEn { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
