using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface ILlmExtractionService
{
    bool IsConfigured { get; }

    Task<List<ExtractedQuestion>> ExtractQuestionsAsync(string text, string? instructions = null, CancellationToken ct = default);

    Task<ExtractedTheory> ExtractTheoryAsync(string text, string? instructions = null, CancellationToken ct = default);
}
