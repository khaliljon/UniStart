using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

[ApiController]
[Route("api/news")]
public class NewsController : ControllerBase
{
    private readonly UniStartDbContext _db;

    public NewsController(UniStartDbContext db)
    {
        _db = db;
    }

    private static NewsArticleDto ToDto(NewsArticle n) => new(
        n.Id, n.Title, n.Summary, n.Body, n.ImageUrl,
        n.IsPublished, n.PublishedAt, n.CreatedAt, n.UpdatedAt,
        n.TitleKz, n.TitleEn, n.SummaryKz, n.SummaryEn, n.BodyKz, n.BodyEn,
        n.Slug, n.Category, n.IsFeatured);

    // Transliterates Cyrillic and strips punctuation so titles become clean URLs.
    private static string Slugify(string title)
    {
        const string cyr = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";
        string[] lat = { "a","b","v","g","d","e","e","zh","z","i","y","k","l","m","n","o","p","r","s","t","u","f","h","c","ch","sh","sch","","y","","e","yu","ya" };
        var sb = new System.Text.StringBuilder();
        foreach (var ch in title.ToLowerInvariant())
        {
            var idx = cyr.IndexOf(ch);
            if (idx >= 0) sb.Append(lat[idx]);
            else if (char.IsLetterOrDigit(ch) && ch < 128) sb.Append(ch);
            else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_') sb.Append('-');
        }
        var slug = System.Text.RegularExpressions.Regex.Replace(sb.ToString(), "-{2,}", "-").Trim('-');
        return slug.Length > 180 ? slug[..180].Trim('-') : slug;
    }

    private async Task<string> UniqueSlugAsync(string title, int? excludeId = null)
    {
        var baseSlug = Slugify(title);
        if (string.IsNullOrEmpty(baseSlug)) baseSlug = "news";
        var slug = baseSlug;
        var i = 2;
        while (await IsSlugTakenAsync(slug, excludeId))
            slug = $"{baseSlug}-{i++}";
        return slug;
    }

    /// <summary>A slug is taken if another article uses it now or used it before.</summary>
    private async Task<bool> IsSlugTakenAsync(string slug, int? excludeId)
    {
        var byCurrent = await _db.NewsArticles.AnyAsync(x => x.Slug == slug && (excludeId == null || x.Id != excludeId));
        if (byCurrent) return true;
        return await _db.NewsSlugHistories.AnyAsync(h => h.Slug == slug && (excludeId == null || h.NewsArticleId != excludeId));
    }

    private static readonly System.Text.RegularExpressions.Regex SlugFormat =
        new("^[a-z0-9]+(?:-[a-z0-9]+)*$", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>Validates a manually entered slug. Returns an error message, or null when valid.</summary>
    private async Task<string?> ValidateSlugAsync(string slug, int? excludeId)
    {
        if (slug.Length > 180)
            return "Slug слишком длинный (максимум 180 символов).";
        if (!SlugFormat.IsMatch(slug))
            return "Slug может содержать только латинские буквы в нижнем регистре, цифры и дефисы. Например: csca-registration.";
        if (await IsSlugTakenAsync(slug, excludeId))
            return "Такой slug уже используется другой новостью.";
        return null;
    }

    /// <summary>Resolves the requested slug: normalizes it or generates one from the title.</summary>
    private async Task<(string? Slug, string? Error)> ResolveSlugAsync(string? requested, string title, int? excludeId)
    {
        var slug = requested?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(slug))
            return (await UniqueSlugAsync(title, excludeId), null);

        var error = await ValidateSlugAsync(slug, excludeId);
        return error != null ? (null, error) : (slug, null);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] int? limit)
    {
        var query = _db.NewsArticles
            .Where(n => n.IsPublished)
            .OrderByDescending(n => n.PublishedAt ?? n.CreatedAt)
            .AsQueryable();

        if (limit is > 0 and <= 50)
            query = query.Take(limit.Value);

        var items = await query.ToListAsync();
        return Ok(items.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var n = await _db.NewsArticles.FindAsync(id);
        if (n == null || !n.IsPublished) return NotFound();
        return Ok(ToDto(n));
    }

    /// <summary>
    /// Public article lookup by slug. Retired slugs resolve to their article and answer 301
    /// so old links keep working; unknown slugs return 404.
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var key = slug.Trim().ToLowerInvariant();

        var n = await _db.NewsArticles.FirstOrDefaultAsync(x => x.Slug == key && x.IsPublished);
        if (n != null) return Ok(ToDto(n));

        // Retired slug → permanent redirect to the current one (never chained: history always
        // points at the article, so one hop always lands on the canonical slug).
        var history = await _db.NewsSlugHistories
            .Include(h => h.NewsArticle)
            .FirstOrDefaultAsync(h => h.Slug == key);
        if (history?.NewsArticle is { IsPublished: true } target)
        {
            Response.Headers.Location = $"/api/news/by-slug/{target.Slug}";
            return StatusCode(StatusCodes.Status301MovedPermanently, ToDto(target));
        }

        if (int.TryParse(key, out var id))
        {
            var byId = await _db.NewsArticles.FirstOrDefaultAsync(x => x.Id == id && x.IsPublished);
            if (byId != null) return Ok(ToDto(byId));
        }

        return NotFound();
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListAll()
    {
        var items = await _db.NewsArticles
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
        return Ok(items.Select(ToDto));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] NewsUpsertDto dto)
    {
        var (slug, slugError) = await ResolveSlugAsync(dto.Slug, dto.Title, excludeId: null);
        if (slugError != null) return BadRequest(new { field = "slug", error = slugError });

        var article = new NewsArticle
        {
            Title = dto.Title.Trim(),
            Summary = dto.Summary.Trim(),
            Body = dto.Body,
            TitleKz = string.IsNullOrWhiteSpace(dto.TitleKz) ? null : dto.TitleKz.Trim(),
            TitleEn = string.IsNullOrWhiteSpace(dto.TitleEn) ? null : dto.TitleEn.Trim(),
            SummaryKz = string.IsNullOrWhiteSpace(dto.SummaryKz) ? null : dto.SummaryKz.Trim(),
            SummaryEn = string.IsNullOrWhiteSpace(dto.SummaryEn) ? null : dto.SummaryEn.Trim(),
            BodyKz = string.IsNullOrWhiteSpace(dto.BodyKz) ? null : dto.BodyKz,
            BodyEn = string.IsNullOrWhiteSpace(dto.BodyEn) ? null : dto.BodyEn,
            ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim(),
            IsPublished = dto.IsPublished,
            PublishedAt = dto.IsPublished ? DateTime.UtcNow : null,
            Category = NewsCategories.Normalize(dto.Category),
            IsFeatured = dto.IsFeatured,
            Slug = slug!,
        };
        _db.NewsArticles.Add(article);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = article.Id }, ToDto(article));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] NewsUpsertDto dto)
    {
        var article = await _db.NewsArticles.FindAsync(id);
        if (article == null) return NotFound();

        var (slug, slugError) = await ResolveSlugAsync(dto.Slug, dto.Title, excludeId: article.Id);
        if (slugError != null) return BadRequest(new { field = "slug", error = slugError });

        var wasPublished = article.IsPublished;
        article.Title = dto.Title.Trim();
        article.Summary = dto.Summary.Trim();
        article.Body = dto.Body;
        article.TitleKz = string.IsNullOrWhiteSpace(dto.TitleKz) ? null : dto.TitleKz.Trim();
        article.TitleEn = string.IsNullOrWhiteSpace(dto.TitleEn) ? null : dto.TitleEn.Trim();
        article.SummaryKz = string.IsNullOrWhiteSpace(dto.SummaryKz) ? null : dto.SummaryKz.Trim();
        article.SummaryEn = string.IsNullOrWhiteSpace(dto.SummaryEn) ? null : dto.SummaryEn.Trim();
        article.BodyKz = string.IsNullOrWhiteSpace(dto.BodyKz) ? null : dto.BodyKz;
        article.BodyEn = string.IsNullOrWhiteSpace(dto.BodyEn) ? null : dto.BodyEn;
        article.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl) ? null : dto.ImageUrl.Trim();
        article.IsPublished = dto.IsPublished;
        article.Category = NewsCategories.Normalize(dto.Category);
        article.IsFeatured = dto.IsFeatured;

        if (!string.Equals(article.Slug, slug, StringComparison.Ordinal))
        {
            var previous = article.Slug;
            // The new slug may be one this article used before — drop it from history so the
            // redirect chain can never point back at itself.
            var reclaimed = await _db.NewsSlugHistories
                .FirstOrDefaultAsync(h => h.Slug == slug && h.NewsArticleId == article.Id);
            if (reclaimed != null) _db.NewsSlugHistories.Remove(reclaimed);

            article.Slug = slug!;

            if (!string.IsNullOrWhiteSpace(previous))
                _db.NewsSlugHistories.Add(new NewsSlugHistory { Slug = previous, NewsArticleId = article.Id });
        }
        if (dto.IsPublished && !wasPublished)
            article.PublishedAt = DateTime.UtcNow;
        else if (!dto.IsPublished)
            article.PublishedAt = null;

        await _db.SaveChangesAsync();
        return Ok(ToDto(article));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await _db.NewsArticles.FindAsync(id);
        if (article == null) return NotFound();
        _db.NewsArticles.Remove(article);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
