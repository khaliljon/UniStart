namespace UniStart.Application.DTOs;

/// <summary>Public-facing legal document payload.</summary>
public record LegalDocumentDto(
    string Slug,
    string Title,
    string LastUpdatedLabel,
    string Content,
    DateTime? UpdatedAt,
    string? TitleKz = null,
    string? TitleEn = null,
    string? ContentKz = null,
    string? ContentEn = null,
    string? LastUpdatedLabelKz = null,
    string? LastUpdatedLabelEn = null);

/// <summary>Admin update payload for a legal document.</summary>
public record UpdateLegalDocumentDto(
    string Title,
    string LastUpdatedLabel,
    string Content,
    string? TitleKz = null,
    string? TitleEn = null,
    string? ContentKz = null,
    string? ContentEn = null,
    string? LastUpdatedLabelKz = null,
    string? LastUpdatedLabelEn = null);
