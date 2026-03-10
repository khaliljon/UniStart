using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class StrategyService : IStrategyService
{
    private readonly UniStartDbContext _context;

    public StrategyService(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StrategyGuideSummaryDto>> GetGuidesByExamAsync(int userId, string[]? examTypeCodes = null)
    {
        var query = _context.StrategyGuides.AsQueryable();

        if (examTypeCodes != null && examTypeCodes.Length > 0)
            query = query.Where(g => examTypeCodes.Contains(g.ExamTypeCode));

        var guides = await query
            .OrderBy(g => g.SortOrder)
            .ToListAsync();

        var readGuideIds = await _context.UserGuideProgress
            .Where(p => p.UserId == userId && guides.Select(g => g.Id).Contains(p.GuideId))
            .Select(p => p.GuideId)
            .ToListAsync();

        return guides.Select(g => new StrategyGuideSummaryDto(
            g.Id, g.ExamTypeCode, g.Title, g.Summary, g.Category, g.EstimatedReadMinutes,
            readGuideIds.Contains(g.Id)
        ));
    }

    public async Task<StrategyGuideDto?> GetGuideAsync(int userId, int guideId)
    {
        var guide = await _context.StrategyGuides.FindAsync(guideId);
        if (guide == null) return null;

        var isRead = await _context.UserGuideProgress
            .AnyAsync(p => p.UserId == userId && p.GuideId == guideId);

        return new StrategyGuideDto(
            guide.Id, guide.ExamTypeCode, guide.Title, guide.Summary,
            guide.Content, guide.Category, guide.EstimatedReadMinutes, isRead
        );
    }

    public async Task MarkReadAsync(int userId, int guideId)
    {
        var exists = await _context.UserGuideProgress
            .AnyAsync(p => p.UserId == userId && p.GuideId == guideId);

        if (!exists)
        {
            _context.UserGuideProgress.Add(new UserGuideProgress
            {
                UserId = userId,
                GuideId = guideId,
                ReadAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}
