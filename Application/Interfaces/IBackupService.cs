using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IBackupService
{
    Task<BackupFileDto> CreateBackupAsync(string trigger = "manual");

    IReadOnlyList<BackupFileDto> ListBackups();

    string? ResolveBackupPath(string fileName);

    bool DeleteBackup(string fileName);
}
