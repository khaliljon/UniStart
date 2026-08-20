using Microsoft.AspNetCore.Http;

namespace UniStart.Application.Interfaces;

public interface IImageUploadService
{
    Task<string> UploadAsync(IFormFile file, CancellationToken ct = default);

    Task<string> UploadBytesAsync(byte[] data, string contentType, CancellationToken ct = default);

    PdfUploadTarget CreatePdfUploadTarget();
}

public record PdfUploadTarget(string UploadUrl, string PublicUrl);
