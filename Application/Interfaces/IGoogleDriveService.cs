namespace UniStart.Application.Interfaces;

/// <summary>A file or folder discovered while walking a Google Drive tree.</summary>
public record DriveNode(
    string Id,
    string Name,
    string MimeType,
    bool IsFolder,
    string? Md5Checksum,
    DateTime? ModifiedAt,
    string FolderPath
);

/// <summary>
/// Thin read-only wrapper over the Google Drive API (service-account auth).
/// Used by the content sync to enumerate and download study-pack files.
/// </summary>
public interface IGoogleDriveService
{
    bool IsConfigured { get; }

    /// <summary>Recursively list all files (not folders) under a root folder.</summary>
    Task<List<DriveNode>> ListFilesRecursiveAsync(string rootFolderId, CancellationToken ct = default);

    /// <summary>
    /// Download a file as plain text. Google Docs are exported to text/plain;
    /// other types are downloaded as-is and decoded as UTF-8.
    /// </summary>
    Task<string> DownloadTextAsync(string fileId, string mimeType, CancellationToken ct = default);

    /// <summary>Download raw bytes (for binaries like PDF that need the file parser).</summary>
    Task<byte[]> DownloadBytesAsync(string fileId, CancellationToken ct = default);
}
