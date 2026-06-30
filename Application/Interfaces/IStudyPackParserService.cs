using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

/// <summary>
/// LLM-based parser that converts raw study-pack / past-paper files into the
/// normalized <see cref="IngestContentDto"/> shape.
/// </summary>
public interface IStudyPackParserService
{
    /// <summary>True when an LLM API key is configured.</summary>
    bool IsConfigured { get; }

    /// <summary>Parse one study-pack file into a single skill payload.</summary>
    Task<IngestContentDto> ParseAsync(
        string text, string examTypeCode, string examSectionName, CancellationToken ct = default);

    /// <summary>
    /// Parse a paired questions + answer-key file set (e.g. TSA / Critical Thinking),
    /// distributing the questions across the supplied skill units.
    /// </summary>
    Task<IReadOnlyList<IngestContentDto>> ParseTsaPairAsync(
        string questionsText, string answersText, string examTypeCode, string examSectionName,
        IReadOnlyList<string> unitSkillNames, IReadOnlyDictionary<string, string>? unitGlossary = null,
        CancellationToken ct = default);
}
