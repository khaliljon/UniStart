using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IStudyPackParserService
{
    bool IsConfigured { get; }

    Task<IngestContentDto> ParseAsync(
        string text, string examTypeCode, string examSectionName, CancellationToken ct = default);

    Task<IReadOnlyList<IngestContentDto>> ParseTsaPairAsync(
        string questionsText, string answersText, string examTypeCode, string examSectionName,
        IReadOnlyList<string> unitSkillNames, IReadOnlyDictionary<string, string>? unitGlossary = null,
        CancellationToken ct = default);
}
