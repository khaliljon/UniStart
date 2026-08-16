using Microsoft.AspNetCore.Mvc;
using UniStart.Application.Services;

namespace UniStart.Controllers;

[ApiController]
[Route("dev/emails")]
public class EmailPreviewController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly ILoggerFactory _loggerFactory;

    public EmailPreviewController(IWebHostEnvironment env, IConfiguration config, ILoggerFactory loggerFactory)
    {
        _env = env;
        _config = config;
        _loggerFactory = loggerFactory;
    }

    private EmailService BuildRenderer() =>
        new(_config, _loggerFactory.CreateLogger<EmailService>());

    [HttpGet]
    public IActionResult Index()
    {
        if (!_env.IsDevelopment()) return NotFound();

        var links = string.Join("", EmailService.PreviewKeys.Select(k =>
            $@"<li style=""margin:8px 0;""><a href=""/dev/emails/{k.Key}"" target=""_blank"" style=""color:#6c5ce7;font-size:16px;text-decoration:none;"">{k.Label}</a> <span style=""color:#999;font-size:13px;"">({k.Key})</span></li>"));

        var html = $@"<!DOCTYPE html>
<html lang=""ru""><head><meta charset=""UTF-8""><title>Email previews</title></head>
<body style=""font-family:'Segoe UI',Roboto,sans-serif;background:#f4f6f9;padding:32px;"">
  <div style=""max-width:600px;margin:0 auto;background:#fff;border-radius:12px;padding:32px;box-shadow:0 2px 12px rgba(0,0,0,0.08);"">
    <h1 style=""color:#1a1a2e;margin:0 0 8px;"">Предпросмотр писем</h1>
    <p style=""color:#666;margin:0 0 24px;"">Локальный просмотр всех email-шаблонов с тестовыми данными.</p>
    <ul style=""list-style:none;padding:0;margin:0;"">{links}</ul>
  </div>
</body></html>";

        return Content(html, "text/html");
    }

    [HttpGet("{key}")]
    public IActionResult Preview(string key)
    {
        if (!_env.IsDevelopment()) return NotFound();

        try
        {
            var html = BuildRenderer().RenderPreview(key);
            return Content(html, "text/html");
        }
        catch (ArgumentException)
        {
            return NotFound($"Unknown email template: {key}");
        }
    }
}
