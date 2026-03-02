using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class FormulaService : IFormulaService
{
    private readonly UniStartDbContext _context;

    public FormulaService(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FormulaCardDto>> GetFormulasByExamAsync(int userId, string examTypeCode)
    {
        var bookmarkedIds = await _context.UserFormulaBookmarks
            .Where(b => b.UserId == userId)
            .Select(b => b.FormulaCardId)
            .ToListAsync();

        var formulas = await _context.FormulaCards
            .Include(f => f.Topic)
                .ThenInclude(t => t.Section)
            .Where(f => f.Topic.Section != null && f.Topic.Section.ExamTypeCode == examTypeCode)
            .OrderBy(f => f.Topic.Name)
            .ThenBy(f => f.SortOrder)
            .Select(f => new FormulaCardDto(
                f.Id,
                f.TopicId,
                f.Topic.Name,
                f.Title,
                f.Formula,
                f.Description,
                bookmarkedIds.Contains(f.Id)
            ))
            .ToListAsync();

        return formulas;
    }

    public async Task<IEnumerable<FormulaCardDto>> GetBookmarkedFormulasAsync(int userId)
    {
        var formulas = await _context.UserFormulaBookmarks
            .Where(b => b.UserId == userId)
            .Include(b => b.FormulaCard)
                .ThenInclude(f => f.Topic)
            .OrderBy(b => b.CreatedAt)
            .Select(b => new FormulaCardDto(
                b.FormulaCard.Id,
                b.FormulaCard.TopicId,
                b.FormulaCard.Topic.Name,
                b.FormulaCard.Title,
                b.FormulaCard.Formula,
                b.FormulaCard.Description,
                true
            ))
            .ToListAsync();

        return formulas;
    }

    public async Task<bool> ToggleBookmarkAsync(int userId, int formulaCardId)
    {
        var existing = await _context.UserFormulaBookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.FormulaCardId == formulaCardId);

        if (existing != null)
        {
            _context.UserFormulaBookmarks.Remove(existing);
            await _context.SaveChangesAsync();
            return false; // removed
        }

        _context.UserFormulaBookmarks.Add(new Domain.Entities.UserFormulaBookmark
        {
            UserId = userId,
            FormulaCardId = formulaCardId
        });
        await _context.SaveChangesAsync();
        return true; // added
    }
}
