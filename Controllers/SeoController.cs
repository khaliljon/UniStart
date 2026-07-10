using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

[ApiController]
public class SeoController : ControllerBase
{
    private readonly UniStartDbContext _db;

    public SeoController(UniStartDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Dynamic robots.txt — returns correct Sitemap URL based on Host header
    /// </summary>
    [HttpGet("/robots.txt")]
    [ResponseCache(Duration = 3600)]
    public IActionResult Robots()
    {
        var host = Request.Host.Value ?? "unistart.kz";
        var origin = $"https://{host}";

        var isSubdomain = host.Contains('.') &&
            !host.Equals("unistart.kz", StringComparison.OrdinalIgnoreCase) &&
            !host.StartsWith("www.", StringComparison.OrdinalIgnoreCase);

        string content;
        if (isSubdomain)
        {
            // Subdomain: allow only public pages, point sitemap to this subdomain
            content = $"""
                User-agent: *
                Allow: /
                Sitemap: {origin}/sitemap.xml

                Disallow: /api/
                Disallow: /login
                Disallow: /register
                Disallow: /onboarding
                Disallow: /profile
                """;
        }
        else
        {
            // Main domain: full robots.txt
            content = $"""
                User-agent: *
                Allow: /
                Sitemap: {origin}/sitemap.xml

                # Disallow admin/API/internal routes
                Disallow: /api/
                Disallow: /hangfire
                Disallow: /login
                Disallow: /register
                Disallow: /verify-email
                Disallow: /onboarding
                Disallow: /test
                Disallow: /learn
                Disallow: /analytics
                Disallow: /study-plan
                Disallow: /mock-exam
                Disallow: /prediction
                Disallow: /profile
                Disallow: /admin
                Disallow: /tutor
                Disallow: /messages
                """;
        }

        return Content(content.TrimStart(), "text/plain");
    }

    /// <summary>
    /// Dynamic sitemap.xml — generates correct URLs based on Host header.
    /// For subdomains: just the landing page.
    /// For main domain: public pages + all active school subdomains.
    /// </summary>
    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 3600)]
    public async Task<IActionResult> Sitemap()
    {
        var host = Request.Host.Value ?? "unistart.kz";
        var origin = $"https://{host}";

        var isSubdomain = host.Contains('.') &&
            !host.Equals("unistart.kz", StringComparison.OrdinalIgnoreCase) &&
            !host.StartsWith("www.", StringComparison.OrdinalIgnoreCase);

        var urls = new List<(string loc, string freq, string priority)>();

        if (isSubdomain)
        {
            urls.Add(($"{origin}/", "weekly", "1.0"));
            urls.Add(($"{origin}/landing", "weekly", "0.9"));
        }
        else
        {
            // Main domain pages
            urls.Add(($"{origin}/", "weekly", "1.0"));
            urls.Add(($"{origin}/landing", "weekly", "0.9"));
            urls.Add(($"{origin}/privacy", "monthly", "0.3"));
            urls.Add(($"{origin}/terms", "monthly", "0.3"));
        }

        var urlEntries = string.Join("\n", urls.Select(u => $"""
              <url>
                <loc>{u.loc}</loc>
                <changefreq>{u.freq}</changefreq>
                <priority>{u.priority}</priority>
              </url>
            """));

        var xml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
            {urlEntries}
            </urlset>
            """;

        return Content(xml.TrimStart(), "application/xml");
    }

    /// <summary>
    /// Returns a minimal HTML page with dynamic OG meta tags for social media crawlers.
    /// Nginx routes bot user-agents here for subdomain requests.
    /// </summary>
    [HttpGet("/og")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> OpenGraph()
    {
        var host = Request.Host.Value ?? "unistart.kz";
        var origin = $"https://{host}";

        var isSubdomain = host.Contains('.') &&
            !host.Equals("unistart.kz", StringComparison.OrdinalIgnoreCase) &&
            !host.StartsWith("www.", StringComparison.OrdinalIgnoreCase);

        string title = "UniStart — Подготовка к экзамену CSCA";
        string description = "Платформа подготовки к экзамену CSCA (China Scholastic Competency Assessment): пробные тесты с ИИ-объяснениями, официальные материалы и новости для поступления в университеты Китая.";
        string image = $"{origin}/og-image.png";
        string siteName = "UniStart";

        var safeTitle = System.Net.WebUtility.HtmlEncode(title);
        var safeDesc = System.Net.WebUtility.HtmlEncode(description);
        var safeSite = System.Net.WebUtility.HtmlEncode(siteName);

        var html = $"""
            <!DOCTYPE html>
            <html lang="ru">
            <head>
            <meta charset="utf-8"/>
            <title>{safeTitle}</title>
            <meta name="description" content="{safeDesc}"/>
            <meta property="og:title" content="{safeTitle}"/>
            <meta property="og:description" content="{safeDesc}"/>
            <meta property="og:type" content="website"/>
            <meta property="og:url" content="{origin}"/>
            <meta property="og:image" content="{System.Net.WebUtility.HtmlEncode(image)}"/>
            <meta property="og:site_name" content="{safeSite}"/>
            <meta property="og:locale" content="ru_RU"/>
            <meta name="twitter:card" content="summary_large_image"/>
            <meta name="twitter:title" content="{safeTitle}"/>
            <meta name="twitter:description" content="{safeDesc}"/>
            <meta name="twitter:image" content="{System.Net.WebUtility.HtmlEncode(image)}"/>
            </head>
            <body><p>{safeDesc}</p></body>
            </html>
            """;

        return Content(html.TrimStart(), "text/html");
    }
}
