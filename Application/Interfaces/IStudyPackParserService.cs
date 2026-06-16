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

    /// <summary>
    /// Parse a TSA past-paper PAIR (a questions file + its answer-key file) and
    /// distribute the paired questions across the fixed Critical-Thinking units.
    /// Returns one payload per unit (skillName = unit name). Does NOT write to the DB.
    /// </summary>
    Task<IReadOnlyList<IngestContentDto>> ParseTsaPairAsync(
        string questionsText,
        string answersText,
        string examTypeCode,
        string examSectionName,
        IReadOnlyList<string> unitSkillNames,
        IReadOnlyDictionary<string, string>? unitGlossary = null,
        CancellationToken ct = default);
}
