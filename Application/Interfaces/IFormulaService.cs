using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IFormulaService
{
    Task<IEnumerable<FormulaCardDto>> GetFormulasByExamAsync(int userId, string[]? examTypeCodes = null);
    Task<IEnumerable<FormulaCardDto>> GetBookmarkedFormulasAsync(int userId);
    Task<bool> ToggleBookmarkAsync(int userId, int formulaCardId);
}
