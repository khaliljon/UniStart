using Microsoft.AspNetCore.Http;

namespace UniStart.Application.Interfaces;

public interface IImageUploadService
{
    Task<string> UploadAsync(IFormFile file, CancellationToken ct = default);

    /// <summary>Upload a PDF document (large files allowed) and return its public URL.</summary>
    Task<string> UploadPdfAsync(IFormFile file, CancellationToken ct = default);
}
