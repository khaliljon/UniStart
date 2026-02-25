using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionStatusDto> GetStatusAsync(int userId);
    Task<DailyUsageDto> GetDailyUsageAsync(int userId);
    Task<bool> HasAccessAsync(int userId, string feature);
    Task<bool> CanAnswerQuestionAsync(int userId);
    Task<UpgradeResponseDto> UpgradeAsync(int userId, UpgradeRequestDto dto);
    TierLimitsDto GetLimits(string tier);
}
