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
        var scheme = Request.Scheme;
        var origin = $"{scheme}://{host}";

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
        var scheme = Request.Scheme;
        var origin = $"{scheme}://{host}";

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
            urls.Add(($"{origin}/tutors", "weekly", "0.8"));
            urls.Add(($"{origin}/privacy", "monthly", "0.3"));
            urls.Add(($"{origin}/terms", "monthly", "0.3"));

            // All active schools with subscription as subdomain entries
            var schools = await _db.TutorSchools
                .Where(s => s.IsActive && s.SubscriptionExpiresAt != null && s.SubscriptionExpiresAt > DateTime.UtcNow)
                .Select(s => s.Slug)
                .ToListAsync();

            foreach (var slug in schools)
            {
                urls.Add(($"{scheme}://{slug}.unistart.kz/", "weekly", "0.8"));
            }
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
}
