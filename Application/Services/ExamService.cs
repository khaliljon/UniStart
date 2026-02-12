using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class ExamService : IExamService
{
    private readonly UniStartDbContext _context;

    public ExamService(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ExamTypeDto>> GetAllExamsAsync()
    {
        var exams = await _context.ExamTypes.ToListAsync();
        return exams.Select(e => new ExamTypeDto(e.Code, e.Name));
    }

    public async Task<ExamWithSectionsDto?> GetExamWithSectionsAsync(string examCode)
    {
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
    }

    public async Task<IEnumerable<ExamSectionDto>> GetExamSectionsAsync(string examCode)
    {
        var sections = await _context.ExamSections
            .Where(s => s.ExamTypeCode == examCode)
            .ToListAsync();

        return sections.Select(s => new ExamSectionDto(
            s.Id,
            s.ExamTypeCode,
            s.Name,
            s.MinScore,
            s.MaxScore
        ));
    }
}
