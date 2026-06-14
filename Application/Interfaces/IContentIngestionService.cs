using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

/// <summary>
/// Idempotent ingestion of normalized study-pack content (Variant B: Topic = concept).
/// Get-or-create Skill/Topic by name; questions deduplicated by content hash.
/// </summary>
public interface IContentIngestionService
{
    Task<IngestResultDto> IngestAsync(IngestContentDto payload);
}
