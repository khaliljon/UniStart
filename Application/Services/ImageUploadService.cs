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
    private const long MaxBytes = 5 * 1024 * 1024;

    private readonly string _endpoint;
    private readonly string _keyId;
    private readonly string _secret;
    private readonly string _bucket;
    private readonly string _publicUrl;

    private AmazonS3Client? _s3;
    private readonly object _lock = new();

    public ImageUploadService(IConfiguration configuration)
    {
        var section = configuration.GetSection("R2");
        _endpoint  = section["Endpoint"] ?? "";
        _keyId     = section["AccessKeyId"] ?? "";
        _secret    = section["SecretAccessKey"] ?? "";
        _bucket    = section["BucketName"] ?? "";
        _publicUrl = (section["PublicUrl"] ?? "").TrimEnd('/');
    }

    private AmazonS3Client GetClient()
    {
        if (_s3 is not null) return _s3;
        lock (_lock)
        {
            if (_s3 is not null) return _s3;
            if (string.IsNullOrWhiteSpace(_keyId))
                throw new InvalidOperationException("R2:AccessKeyId is not configured. Set R2__AccessKeyId environment variable.");
            if (string.IsNullOrWhiteSpace(_secret))
                throw new InvalidOperationException("R2:SecretAccessKey is not configured. Set R2__SecretAccessKey environment variable.");
            if (string.IsNullOrWhiteSpace(_endpoint))
                throw new InvalidOperationException("R2:Endpoint is not configured.");
            if (string.IsNullOrWhiteSpace(_bucket))
                throw new InvalidOperationException("R2:BucketName is not configured.");

            var credentials = new BasicAWSCredentials(_keyId, _secret);
            var config = new AmazonS3Config
            {
                ServiceURL           = _endpoint,
                ForcePathStyle       = true,
                SignatureVersion      = "4",
                AuthenticationRegion = "auto",
            };
            _s3 = new AmazonS3Client(credentials, config);
            return _s3;
        }
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
            throw new InvalidOperationException("R2:PublicUrl is not configured. Set R2__PublicUrl environment variable.");

        var client = GetClient();

        var ext = _mimeToExt.TryGetValue(file.ContentType, out var e) ? e : ".jpg";
        var key = $"questions/{Guid.NewGuid():N}{ext}";

        using var stream = file.OpenReadStream();
        var request = new PutObjectRequest
        {
            BucketName            = _bucket,
            Key                   = key,
            InputStream           = stream,
            ContentType           = file.ContentType,
            DisablePayloadSigning = true,
        };

        try
        {
            await client.PutObjectAsync(request, ct);
        }
        catch (Amazon.S3.AmazonS3Exception ex)
        {
            throw new InvalidOperationException($"R2 upload failed ({(int)ex.StatusCode}): {ex.Message}", ex);
        }

        return $"{_publicUrl}/{key}";
    }

    public PdfUploadTarget CreatePdfUploadTarget()
    {
        if (string.IsNullOrWhiteSpace(_publicUrl))
            throw new InvalidOperationException("R2:PublicUrl is not configured. Set R2__PublicUrl environment variable.");

        var client = GetClient();
        var key = $"materials/{Guid.NewGuid():N}.pdf";

        var request = new GetPreSignedUrlRequest
        {
            BucketName  = _bucket,
            Key         = key,
            Verb        = HttpVerb.PUT,
            Expires     = DateTime.UtcNow.AddMinutes(30),
            ContentType = "application/pdf",
        };

        var uploadUrl = client.GetPreSignedURL(request);
        return new PdfUploadTarget(uploadUrl, $"{_publicUrl}/{key}");
    }

    public void Dispose()
    {
        lock (_lock) { _s3?.Dispose(); _s3 = null; }
    }
}
