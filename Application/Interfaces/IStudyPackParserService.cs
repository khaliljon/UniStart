using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

/// <summary>
/// Parses a raw study-pack file (markdown/plain text) into the normalized
/// <see cref="IngestContentDto"/> shape using an LLM (Variant B: Topic = concept).
/// Produces a preview that an admin reviews before calling the ingest endpoint.
/// </summary>
public interface IStudyPackParserService
{
    bool IsConfigured { get; }

    /// <summary>
    /// Convert one study-pack file's text into a normalized ingest payload.
    /// Does NOT write to the database.
    /// </summary>
    Task<IngestContentDto> ParseAsync(
        string text,
        string examTypeCode,
        string examSectionName,
        CancellationToken ct = default);
}
