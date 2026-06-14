using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

/// <summary>
/// Orchestrates a Google Drive → content sync as a Hangfire background job.
/// Walks the folder tree, parses each new/changed study-pack file via the LLM,
/// and ingests it idempotently. Files unchanged since last run are skipped;
/// files that disappeared from Drive are marked orphaned.
/// </summary>
public interface IDriveSyncService
{
    /// <summary>
    /// Hangfire entry point. Syncs every study-pack file under <paramref name="rootFolderId"/>,
    /// mapping all of them under the given exam type/section.
    /// </summary>
    Task SyncFolderAsync(string rootFolderId, string examTypeCode, string examSectionName);

    /// <summary>Returns the current sync items (reconciliation report).</summary>
    Task<List<DriveSyncItemDto>> GetItemsAsync();
}
