using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

/// <summary>
/// LLM-based question extraction service.
/// Uses AI (DeepSeek/OpenAI) to intelligently extract structured questions
/// from raw text — especially useful for OCR output from scanned PDFs.
/// </summary>
public interface ILlmExtractionService
{
    /// <summary>Whether an LLM API key is configured and the service is usable.</summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Extract structured questions from raw text using LLM.
    /// Returns a list of cleanly parsed questions with options, answers, and explanations.
    /// </summary>
    /// <param name="instructions">Optional admin-provided context/instructions (e.g. expected question count, "add explanations where missing").</param>
    Task<List<ExtractedQuestion>> ExtractQuestionsAsync(string text, string? instructions = null, CancellationToken ct = default);
}
