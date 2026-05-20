using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UniStart.Application.Interfaces;

namespace UniStart.Application.Services;

public class ImageUploadService : IImageUploadService, IDisposable
{
    private static readonly HashSet<string> _allowedMime = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };
    private static readonly Dictionary<string, string> _mimeToExt = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"]  = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"]  = ".gif",
    };
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    private readonly AmazonS3Client _s3;
    private readonly string _bucket;
    private readonly string _publicUrl;

    public ImageUploadService(IConfiguration configuration)
    {
        var section = configuration.GetSection("R2");
        var endpoint  = section["Endpoint"] ?? throw new InvalidOperationException("R2:Endpoint not configured");
        var keyId     = section["AccessKeyId"] ?? throw new InvalidOperationException("R2:AccessKeyId not configured");
        var secret    = section["SecretAccessKey"] ?? throw new InvalidOperationException("R2:SecretAccessKey not configured");
        _bucket    = section["BucketName"] ?? throw new InvalidOperationException("R2:BucketName not configured");
        _publicUrl = (section["PublicUrl"] ?? "").TrimEnd('/');

        var credentials = new BasicAWSCredentials(keyId, secret);
        var config = new AmazonS3Config
        {
            ServiceURL         = endpoint,
            ForcePathStyle     = true,
            SignatureVersion   = "4",
            AuthenticationRegion = "auto",
        };
        _s3 = new AmazonS3Client(credentials, config);
    }

    public async Task<string> UploadAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (!_allowedMime.Contains(file.ContentType))
            throw new ArgumentException($"Unsupported file type: {file.ContentType}. Allowed: jpg, png, webp, gif.");

        if (file.Length > MaxBytes)
            throw new ArgumentException("File exceeds maximum allowed size of 5 MB.");

        if (string.IsNullOrWhiteSpace(_publicUrl))
            throw new InvalidOperationException("R2:PublicUrl is not configured. Enable public access on your R2 bucket and set the URL in configuration.");

        var ext = _mimeToExt.TryGetValue(file.ContentType, out var e) ? e : ".jpg";
        var key = $"questions/{Guid.NewGuid():N}{ext}";

        using var stream = file.OpenReadStream();
        var request = new PutObjectRequest
        {
            BucketName  = _bucket,
            Key         = key,
            InputStream = stream,
            ContentType = file.ContentType,
        };

        await _s3.PutObjectAsync(request, ct);

        return $"{_publicUrl}/{key}";
    }

    public void Dispose() => _s3.Dispose();
}
