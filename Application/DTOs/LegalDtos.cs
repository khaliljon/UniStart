namespace UniStart.Application.DTOs;

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
