namespace UniStart.Domain.Entities;

public class StudyMaterial : IAuditable
{
    public int Id { get; set; }

    public string SubjectKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? TitleKz { get; set; }

    public string? TitleEn { get; set; }

    public string? Description { get; set; }

    public string? DescriptionKz { get; set; }

    public string? DescriptionEn { get; set; }

    public string? PdfUrl { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
