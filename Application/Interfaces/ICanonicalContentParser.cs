using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

/// <summary>
/// Deterministic (no-LLM) parser for content authored in the canonical study-pack
/// Markdown format. Produces the same <see cref="IngestContentDto"/> shape as the LLM
/// parser, but is instant, free and 100% reproducible — ideal for bulk-importing the
/// already-structured question bank.
/// </summary>
public interface ICanonicalContentParser
{
    /// <summary>
    /// Parse canonical-format Markdown into a normalized ingest payload.
    /// Does NOT write to the database. Throws <see cref="FormatException"/> with an
    /// actionable message if the document has no skill title or no topics.
    /// </summary>
    IngestContentDto Parse(string text, string examTypeCode, string examSectionName);
}
