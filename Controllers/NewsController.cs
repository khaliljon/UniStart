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
        n.TitleKz, n.TitleEn, n.SummaryKz, n.SummaryEn, n.BodyKz, n.BodyEn);

    /// <summary>Public: published articles, newest first.</summary>
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

    /// <summary>Public: a single published article.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var n = await _db.NewsArticles.FindAsync(id);
        if (n == null || !n.IsPublished) return NotFound();
        return Ok(ToDto(n));
    }

    /// <summary>Admin: all articles (including drafts).</summary>
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
