namespace UniStart.Application.DTOs;

/// <summary>Public-facing legal document payload.</summary>
public record LegalDocumentDto(
    string Slug,
    string Title,
    string LastUpdatedLabel,
    string Content,
    DateTime? UpdatedAt);

/// <summary>Admin update payload for a legal document.</summary>
public record UpdateLegalDocumentDto(
    string Title,
    string LastUpdatedLabel,
    string Content);
