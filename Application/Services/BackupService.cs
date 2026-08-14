using System.Diagnostics;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class BackupService : IBackupService
{
    private const int RetentionCount = 14;
    private const string Prefix = "unistart_";
    private const string Suffix = ".sql.gz";

    private readonly UniStartDbContext _db;
    private readonly ILogger<BackupService> _logger;
    private readonly string _backupDir;

    public BackupService(UniStartDbContext db, ILogger<BackupService> logger)
    {
        _db = db;
        _logger = logger;
        _backupDir = Environment.GetEnvironmentVariable("BACKUP_DIR")
                     ?? Path.Combine(AppContext.BaseDirectory, "backups");
    }

    public async Task<BackupFileDto> CreateBackupAsync(string trigger = "manual")
    {
        Directory.CreateDirectory(_backupDir);

        var connectionString = _db.Database.GetConnectionString()
            ?? throw new InvalidOperationException("No database connection string is configured.");
        var csb = new NpgsqlConnectionStringBuilder(connectionString);

        var safeTrigger = string.Concat((trigger ?? "manual").Where(char.IsLetterOrDigit));
        if (safeTrigger.Length == 0) safeTrigger = "manual";
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{Prefix}{safeTrigger}_{timestamp}{Suffix}";
        var filePath = Path.Combine(_backupDir, fileName);

        var psi = new ProcessStartInfo
        {
            FileName = "pg_dump",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        psi.ArgumentList.Add("--no-owner");
        psi.ArgumentList.Add("--no-privileges");
        psi.ArgumentList.Add("--format=plain");
        psi.ArgumentList.Add($"--host={csb.Host}");
        psi.ArgumentList.Add($"--port={csb.Port}");
        if (!string.IsNullOrEmpty(csb.Username)) psi.ArgumentList.Add($"--username={csb.Username}");
        psi.ArgumentList.Add($"--dbname={csb.Database}");
        psi.Environment["PGPASSWORD"] = csb.Password ?? string.Empty;

        Process process;
        try
        {
            process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start pg_dump process.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Не удалось запустить pg_dump. Убедитесь, что postgresql-client установлен в окружении.", ex);
        }

        try
        {
            var stderrTask = process.StandardError.ReadToEndAsync();

            await using (var fileStream = File.Create(filePath))
            await using (var gzip = new GZipStream(fileStream, CompressionLevel.Optimal))
            {
                await process.StandardOutput.BaseStream.CopyToAsync(gzip);
            }

            var stderr = await stderrTask;
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                TryDelete(filePath);
                throw new InvalidOperationException($"pg_dump завершился с ошибкой (код {process.ExitCode}): {stderr}");
            }
        }
        finally
        {
            process.Dispose();
        }

        var info = new FileInfo(filePath);
        if (!info.Exists || info.Length == 0)
        {
            TryDelete(filePath);
            throw new InvalidOperationException("Резервная копия пуста — операция отменена.");
        }

        CleanupOldBackups();
        _logger.LogInformation("Database backup created: {File} ({Bytes} bytes, trigger={Trigger})",
            fileName, info.Length, safeTrigger);

        return new BackupFileDto(fileName, info.Length, info.CreationTimeUtc);
    }

    public IReadOnlyList<BackupFileDto> ListBackups()
    {
        if (!Directory.Exists(_backupDir)) return Array.Empty<BackupFileDto>();
        return new DirectoryInfo(_backupDir)
            .GetFiles($"{Prefix}*{Suffix}")
            .OrderByDescending(f => f.CreationTimeUtc)
            .Select(f => new BackupFileDto(f.Name, f.Length, f.CreationTimeUtc))
            .ToList();
    }

    public string? ResolveBackupPath(string fileName)
    {
        if (!IsSafeFileName(fileName)) return null;
        var path = Path.Combine(_backupDir, fileName);
        return File.Exists(path) ? path : null;
    }

    public bool DeleteBackup(string fileName)
    {
        var path = ResolveBackupPath(fileName);
        if (path == null) return false;
        File.Delete(path);
        _logger.LogInformation("Database backup deleted: {File}", fileName);
        return true;
    }

    private void CleanupOldBackups()
    {
        if (!Directory.Exists(_backupDir)) return;
        var old = new DirectoryInfo(_backupDir)
            .GetFiles($"{Prefix}*{Suffix}")
            .OrderByDescending(f => f.CreationTimeUtc)
            .Skip(RetentionCount);
        foreach (var f in old) TryDelete(f.FullName);
    }

    private static bool IsSafeFileName(string fileName)
        => !string.IsNullOrWhiteSpace(fileName)
           && fileName.StartsWith(Prefix, StringComparison.Ordinal)
           && fileName.EndsWith(Suffix, StringComparison.Ordinal)
           && fileName.IndexOfAny(new[] { '/', '\\' }) < 0
           && !fileName.Contains("..", StringComparison.Ordinal);

    private void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete backup file {Path}", path); }
    }
}
