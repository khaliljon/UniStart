using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IReferralService
{
    Task<ReferralActivateResponseDto> ActivateAsync(int userId);
    Task<ReferralStatsDto> GetMyStatsAsync(int userId);
    Task<List<ReferralUsageDto>> GetMyReferralsAsync(int userId);
    Task GrantRewardForProUpgradeAsync(int userId);
}
