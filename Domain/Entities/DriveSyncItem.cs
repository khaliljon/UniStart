namespace UniStart.Domain.Entities;

/// <summary>
/// Tracks one Google Drive file across sync runs so ingestion is idempotent and
/// resumable. Change detection uses <see cref="Checksum"/> (md5 for binaries,
/// modifiedTime token for Google Docs). Files that vanish from Drive are marked
/// <see cref="DriveSyncStatus.Orphaned"/> rather than deleted.
/// </summary>
public class DriveSyncItem
{
    public int Id { get; set; }

    /// <summary>Google Drive file id (stable, unique).</summary>
    public string DriveFileId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;

    /// <summary>Human-readable folder path within the synced root (for the report).</summary>
    public string? FolderPath { get; set; }

    /// <summary>md5Checksum (binaries) or a modifiedTime/version token (Google Docs).</summary>
    public string? Checksum { get; set; }

    public DateTime? DriveModifiedAt { get; set; }

    public DriveSyncStatus Status { get; set; } = DriveSyncStatus.Pending;

    /// <summary>Name of the Skill this file was ingested into.</summary>
    public string? MappedSkillName { get; set; }
    public int? MappedSkillId { get; set; }

    /// <summary>Serialized last IngestResultDto (counts + warnings).</summary>
    public string? LastResultJson { get; set; }
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSyncedAt { get; set; }
}

public enum DriveSyncStatus
{
    Pending = 0,
    Synced = 1,
    SkippedUnchanged = 2,
    Failed = 3,
    Unmapped = 4,   // a file type we don't ingest (e.g. image/folder)
    Orphaned = 5    // existed before, now gone from Drive
}
