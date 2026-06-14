using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using UniStart.Application.Interfaces;

namespace UniStart.Application.Services;

/// <summary>
/// Read-only Google Drive client using a service account.
///
/// Config (appsettings.json):
///   "DriveSync": {
///     "ServiceAccountJsonPath": "secrets/drive-sa.json",  // or set DriveSync:ServiceAccountJson with inline JSON
///     "ApplicationName": "UniStart"
///   }
///
/// Share the Drive root folder with the service account's email (read access).
/// </summary>
public class GoogleDriveService : IGoogleDriveService
{
    private readonly ILogger<GoogleDriveService> _logger;
    private readonly string? _credentialJson;
    private readonly string _applicationName;

    // Google Docs editor MIME types that must be exported rather than downloaded.
    private const string GoogleDocMime = "application/vnd.google-apps.document";
    private const string GoogleFolderMime = "application/vnd.google-apps.folder";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_credentialJson);

    public GoogleDriveService(IConfiguration config, ILogger<GoogleDriveService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        var section = config.GetSection("DriveSync");
        _applicationName = section["ApplicationName"] ?? "UniStart";

        // Inline JSON wins; otherwise read from a file path (absolute or content-root relative).
        var inline = section["ServiceAccountJson"];
        if (!string.IsNullOrWhiteSpace(inline))
        {
            _credentialJson = inline;
        }
        else
        {
            var path = section["ServiceAccountJsonPath"];
            if (!string.IsNullOrWhiteSpace(path))
            {
                var fullPath = Path.IsPathRooted(path) ? path : Path.Combine(env.ContentRootPath, path);
                if (File.Exists(fullPath))
                    _credentialJson = File.ReadAllText(fullPath);
                else
                    _logger.LogWarning("DriveSync service account file not found at {Path}", fullPath);
            }
        }
    }

    private DriveService CreateService()
    {
        if (!IsConfigured)
            throw new InvalidOperationException("Google Drive is not configured (DriveSync:ServiceAccountJson[Path]).");

        var credential = GoogleCredential
            .FromJson(_credentialJson)
            .CreateScoped(DriveService.ScopeConstants.DriveReadonly);

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _applicationName
        });
    }

    public async Task<List<DriveNode>> ListFilesRecursiveAsync(string rootFolderId, CancellationToken ct = default)
    {
        using var service = CreateService();
        var results = new List<DriveNode>();
        await WalkAsync(service, rootFolderId, string.Empty, results, ct);
        return results;
    }

    private async Task WalkAsync(DriveService service, string folderId, string folderPath, List<DriveNode> sink, CancellationToken ct)
    {
        string? pageToken = null;
        do
        {
            var req = service.Files.List();
            req.Q = $"'{folderId}' in parents and trashed = false";
            req.Fields = "nextPageToken, files(id, name, mimeType, md5Checksum, modifiedTime)";
            req.PageSize = 1000;
            req.PageToken = pageToken;
            // Needed for files located in Shared Drives.
            req.SupportsAllDrives = true;
            req.IncludeItemsFromAllDrives = true;

            var response = await req.ExecuteAsync(ct);
            foreach (var f in response.Files)
            {
                if (f.MimeType == GoogleFolderMime)
                {
                    var childPath = string.IsNullOrEmpty(folderPath) ? f.Name : $"{folderPath}/{f.Name}";
                    await WalkAsync(service, f.Id, childPath, sink, ct);
                }
                else
                {
                    sink.Add(new DriveNode(
                        f.Id,
                        f.Name,
                        f.MimeType ?? string.Empty,
                        IsFolder: false,
                        Md5Checksum: f.Md5Checksum,
                        ModifiedAt: f.ModifiedTimeDateTimeOffset?.UtcDateTime,
                        FolderPath: folderPath));
                }
            }
            pageToken = response.NextPageToken;
        }
        while (!string.IsNullOrEmpty(pageToken) && !ct.IsCancellationRequested);
    }

    public async Task<string> DownloadTextAsync(string fileId, string mimeType, CancellationToken ct = default)
    {
        using var service = CreateService();
        using var ms = new MemoryStream();

        if (mimeType == GoogleDocMime)
        {
            var export = service.Files.Export(fileId, "text/plain");
            await export.DownloadAsync(ms, ct);
        }
        else
        {
            var get = service.Files.Get(fileId);
            get.SupportsAllDrives = true;
            await get.DownloadAsync(ms, ct);
        }

        return Encoding.UTF8.GetString(ms.ToArray());
    }

    public async Task<byte[]> DownloadBytesAsync(string fileId, CancellationToken ct = default)
    {
        using var service = CreateService();
        using var ms = new MemoryStream();
        var get = service.Files.Get(fileId);
        get.SupportsAllDrives = true;
        await get.DownloadAsync(ms, ct);
        return ms.ToArray();
    }
}
