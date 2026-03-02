using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IStrategyService
{
    Task<IEnumerable<StrategyGuideSummaryDto>> GetGuidesByExamAsync(int userId, string examTypeCode);
    Task<StrategyGuideDto?> GetGuideAsync(int userId, int guideId);
    Task MarkReadAsync(int userId, int guideId);
}
