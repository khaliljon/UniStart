using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class ExamService : IExamService
{
    private readonly UniStartDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(15);

    public ExamService(UniStartDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IEnumerable<ExamTypeDto>> GetAllExamsAsync()
    {
        return await _cache.GetOrCreateAsync("exams:all", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            var exams = await _context.ExamTypes.ToListAsync();
            return exams.Select(e => new ExamTypeDto(e.Code, e.Name)).ToList();
        }) ?? [];
    }

    public async Task<ExamWithSectionsDto?> GetExamWithSectionsAsync(string examCode)
    {
        return await _cache.GetOrCreateAsync($"exams:sections:{examCode}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            var exam = await _context.ExamTypes
                .Include(e => e.Sections)
                .FirstOrDefaultAsync(e => e.Code == examCode);

            if (exam == null) return null;

            return new ExamWithSectionsDto(
                exam.Code,
                exam.Name,
                exam.Sections.Select(s => new ExamSectionDto(
                    s.Id,
                    s.ExamTypeCode,
                    s.Name,
                    s.MinScore,
                    s.MaxScore
                ))
            );
        });
    }

    public async Task<IEnumerable<ExamSectionDto>> GetExamSectionsAsync(string examCode)
    {
        return await _cache.GetOrCreateAsync($"exams:exam-sections:{examCode}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            var sections = await _context.ExamSections
                .Where(s => s.ExamTypeCode == examCode)
                .ToListAsync();

            return sections.Select(s => new ExamSectionDto(
                s.Id,
                s.ExamTypeCode,
                s.Name,
                s.MinScore,
                s.MaxScore
            )).ToList();
        }) ?? [];
    }
}
