using Microsoft.AspNetCore.Http;

namespace UniStart.Application.Interfaces;

public interface IImageUploadService
{
    Task<string> UploadAsync(IFormFile file, CancellationToken ct = default);
}
