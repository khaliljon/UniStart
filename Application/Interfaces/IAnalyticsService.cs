using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IAnalyticsService
{
    Task<SkillAnalyticsDto> GetUserAnalyticsAsync(int userId);
    Task<IEnumerable<UserSkillProfileDto>> GetUserSkillsAsync(int userId);
}
