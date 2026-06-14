using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

/// <summary>
/// Hangfire-driven Google Drive → content sync. Idempotent and resumable:
/// each file is tracked in <see cref="DriveSyncItem"/> by a checksum token, so a
/// re-run only re-parses files that actually changed. Parsing/ingestion reuse the
/// same pipeline as the manual /parse + /ingest endpoints.
/// </summary>
public class DriveSyncService : IDriveSyncService
{
    private readonly UniStartDbContext _db;
    private readonly IGoogleDriveService _drive;
    private readonly IStudyPackParserService _parser;
    private readonly IContentIngestionService _ingestion;
    private readonly IFileParserService _fileParser;
    private readonly ILogger<DriveSyncService> _logger;

    private const string GoogleDocMime = "application/vnd.google-apps.document";
    private const string DocxMime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public DriveSyncService(
        UniStartDbContext db,
        IGoogleDriveService drive,
        IStudyPackParserService parser,
        IContentIngestionService ingestion,
        IFileParserService fileParser,
        ILogger<DriveSyncService> logger)
    {
        _db = db;
        _drive = drive;
        _parser = parser;
        _ingestion = ingestion;
        _fileParser = fileParser;
        _logger = logger;
    }

    public async Task SyncFolderAsync(string rootFolderId, string examTypeCode, string examSectionName)
    {
        if (!_drive.IsConfigured)
        {
            _logger.LogError("Drive sync aborted: Google Drive is not configured.");
            return;
        }
        if (!_parser.IsConfigured)
        {
            _logger.LogError("Drive sync aborted: LLM parser is not configured.");
            return;
        }

        _logger.LogInformation("Drive sync started for folder {Folder} ({Exam}/{Section})",
            rootFolderId, examTypeCode, examSectionName);

        var nodes = await _drive.ListFilesRecursiveAsync(rootFolderId);
        _logger.LogInformation("Drive sync: discovered {Count} file(s)", nodes.Count);

        var seenIds = new HashSet<string>();

        foreach (var node in nodes)
        {
            seenIds.Add(node.Id);
            try
            {
                await SyncSingleAsync(node, examTypeCode, examSectionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Drive sync: failed to process {Name} ({Id})", node.Name, node.Id);
                var failed = await _db.DriveSyncItems.FirstOrDefaultAsync(x => x.DriveFileId == node.Id);
                if (failed != null)
                {
                    failed.Status = DriveSyncStatus.Failed;
                    failed.ErrorMessage = Truncate(ex.Message, 1000);
                    failed.LastSyncedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }
        }

        // Mark items that vanished from Drive as orphaned (do not delete content).
        var orphans = await _db.DriveSyncItems
            .Where(x => x.Status != DriveSyncStatus.Orphaned)
            .ToListAsync();
        foreach (var o in orphans.Where(o => !seenIds.Contains(o.DriveFileId)))
        {
            o.Status = DriveSyncStatus.Orphaned;
            o.LastSyncedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();

        _logger.LogInformation("Drive sync finished for folder {Folder}", rootFolderId);
    }

    private async Task SyncSingleAsync(DriveNode node, string examTypeCode, string examSectionName)
    {
        var checksum = ComputeChecksumToken(node);
        var item = await _db.DriveSyncItems.FirstOrDefaultAsync(x => x.DriveFileId == node.Id);
        if (item == null)
        {
            item = new DriveSyncItem
            {
                DriveFileId = node.Id,
                Name = node.Name,
                MimeType = node.MimeType,
                FolderPath = node.FolderPath,
                CreatedAt = DateTime.UtcNow,
            };
            _db.DriveSyncItems.Add(item);
        }

        // Refresh metadata each run.
        item.Name = node.Name;
        item.MimeType = node.MimeType;
        item.FolderPath = node.FolderPath;
        item.DriveModifiedAt = node.ModifiedAt;

        // Unsupported file type → record and skip.
        if (!IsIngestible(node.MimeType, node.Name))
        {
            item.Status = DriveSyncStatus.Unmapped;
            item.LastSyncedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return;
        }

        // Unchanged since last successful sync → skip (idempotent / incremental).
        if (item.Status == DriveSyncStatus.Synced && item.Checksum == checksum)
        {
            item.Status = DriveSyncStatus.SkippedUnchanged;
            item.LastSyncedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return;
        }

        var text = await ExtractTextAsync(node);
        if (string.IsNullOrWhiteSpace(text))
        {
            item.Status = DriveSyncStatus.Failed;
            item.ErrorMessage = "Empty text extracted from file.";
            item.LastSyncedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return;
        }

        var payload = await _parser.ParseAsync(text, examTypeCode, examSectionName);
        var result = await _ingestion.IngestAsync(payload);

        item.Status = DriveSyncStatus.Synced;
        item.Checksum = checksum;
        item.MappedSkillId = result.SkillId;
        item.MappedSkillName = result.SkillName;
        item.LastResultJson = JsonSerializer.Serialize(result);
        item.ErrorMessage = null;
        item.LastSyncedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Drive sync: '{Name}' → skill '{Skill}' (+{Q} questions)",
            node.Name, result.SkillName, result.QuestionsCreated);
    }

    private async Task<string> ExtractTextAsync(DriveNode node)
    {
        var ext = Path.GetExtension(node.Name).ToLowerInvariant();

        if (node.MimeType == GoogleDocMime || ext is ".md" or ".markdown" or ".txt")
            return await _drive.DownloadTextAsync(node.Id, node.MimeType);

        if (ext == ".pdf" || node.MimeType == "application/pdf")
        {
            var bytes = await _drive.DownloadBytesAsync(node.Id);
            using var ms = new MemoryStream(bytes);
            return _fileParser.ParsePdf(ms);
        }

        if (ext == ".docx" || node.MimeType == DocxMime)
        {
            var bytes = await _drive.DownloadBytesAsync(node.Id);
            using var ms = new MemoryStream(bytes);
            return _fileParser.ParseDocx(ms);
        }

        // Fallback: try plain text decode.
        return await _drive.DownloadTextAsync(node.Id, node.MimeType);
    }

    private static bool IsIngestible(string mimeType, string name)
    {
        if (mimeType == GoogleDocMime || mimeType == DocxMime || mimeType == "application/pdf"
            || mimeType == "text/plain" || mimeType == "text/markdown")
            return true;
        var ext = Path.GetExtension(name).ToLowerInvariant();
        return ext is ".md" or ".markdown" or ".txt" or ".pdf" or ".docx";
    }

    /// <summary>md5 for binaries; modifiedTime token for Google Docs (which have no md5).</summary>
    private static string ComputeChecksumToken(DriveNode node)
    {
        if (!string.IsNullOrWhiteSpace(node.Md5Checksum))
            return "md5:" + node.Md5Checksum;
        return "mtime:" + (node.ModifiedAt?.ToString("o") ?? "unknown");
    }

    public async Task<List<DriveSyncItemDto>> GetItemsAsync()
    {
        return await _db.DriveSyncItems
            .OrderBy(x => x.FolderPath).ThenBy(x => x.Name)
            .Select(x => new DriveSyncItemDto(
                x.Id, x.DriveFileId, x.Name, x.MimeType, x.FolderPath,
                x.Status.ToString(), x.MappedSkillName, x.MappedSkillId,
                x.ErrorMessage, x.DriveModifiedAt, x.LastSyncedAt))
            .ToListAsync();
    }

    private static string Truncate(string s, int max)
        => string.IsNullOrEmpty(s) ? s : (s.Length <= max ? s : s[..max]);
}
