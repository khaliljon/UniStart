namespace UniStart.Domain.Entities;

/// <summary>
/// A purchasable PDF study material (textbook) for a CSCA subject.
/// Stored in R2; URL returned only after purchase verification.
/// </summary>
public class StudyMaterial : IAuditable
{
    public int Id { get; set; }

    /// <summary>Subject key matching CSCA_SUBJECTS on frontend: "math" | "physics" | "chemistry" | "chineseTech" | "chineseHum".</summary>
    public string SubjectKey { get; set; } = string.Empty;

    /// <summary>Display title (e.g. "Математика — CSCA 备考教材").</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Kazakh title (optional; falls back to <see cref="Title"/>).</summary>
    public string? TitleKz { get; set; }

    /// <summary>English title (optional; falls back to <see cref="Title"/>).</summary>
    public string? TitleEn { get; set; }

    /// <summary>Short description shown to the user.</summary>
    public string? Description { get; set; }

    /// <summary>Kazakh description (optional; falls back to <see cref="Description"/>).</summary>
    public string? DescriptionKz { get; set; }

    /// <summary>English description (optional; falls back to <see cref="Description"/>).</summary>
    public string? DescriptionEn { get; set; }

    /// <summary>Cloudflare R2 public/signed URL for the PDF file.</summary>
    public string? PdfUrl { get; set; }

    /// <summary>Price in KZT.</summary>
    public decimal Price { get; set; }

    /// <summary>Whether this material is visible/purchasable.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
