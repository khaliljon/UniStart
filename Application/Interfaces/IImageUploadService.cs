using Microsoft.AspNetCore.Http;

namespace UniStart.Application.Interfaces;

public interface IImageUploadService
{
    Task<string> UploadAsync(IFormFile file, CancellationToken ct = default);

    /// <summary>Upload a PDF document (large files allowed) and return its public URL.</summary>
    Task<string> UploadPdfAsync(IFormFile file, CancellationToken ct = default);

    /// <summary>
    /// Creates a short-lived presigned URL the browser can PUT a PDF straight to R2 with,
    /// bypassing the API server (no server memory/bandwidth used for the file).
    /// </summary>
    PdfUploadTarget CreatePdfUploadTarget();
}

/// <summary>A presigned direct-to-R2 upload target for a PDF.</summary>
/// <param name="UploadUrl">Presigned PUT URL the browser uploads the file to.</param>
/// <param name="PublicUrl">The final public URL to store once the upload succeeds.</param>
public record PdfUploadTarget(string UploadUrl, string PublicUrl);
