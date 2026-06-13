using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IBackupService
{
    /// <summary>Create a compressed pg_dump backup of the database.</summary>
    Task<BackupFileDto> CreateBackupAsync(string trigger = "manual");

    /// <summary>List existing backup files, newest first.</summary>
    IReadOnlyList<BackupFileDto> ListBackups();

    /// <summary>Resolve a safe absolute path for a backup file, or null if it does not exist.</summary>
    string? ResolveBackupPath(string fileName);

    /// <summary>Delete a backup file. Returns true if removed.</summary>
    bool DeleteBackup(string fileName);
}
