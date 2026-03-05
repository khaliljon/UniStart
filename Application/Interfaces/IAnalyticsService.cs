using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IAnalyticsService
{
    Task<SkillAnalyticsDto> GetUserAnalyticsAsync(int userId);
    Task<IEnumerable<UserSkillProfileDto>> GetUserSkillsAsync(int userId);
    
    // Stage 4: Enhanced analytics
    Task<DashboardDto> GetDashboardAsync(int userId);
    Task<IEnumerable<SkillHistoryPointDto>> GetSkillHistoryAsync(int userId, int days = 30);
    Task<IEnumerable<DailyActivityDto>> GetActivityHeatmapAsync(int userId, int days = 90);
    Task<IEnumerable<DifficultyStatsDto>> GetDifficultyBreakdownAsync(int userId);
    
    // Test sessions
    Task<TestSessionSummaryDto> StartSessionAsync(int userId, string examTypeCode, string mode);
    Task<TestSessionSummaryDto> CompleteSessionAsync(int userId, int sessionId);
    Task<IEnumerable<TestSessionSummaryDto>> GetSessionsAsync(int userId, int page = 1, int pageSize = 10);
    Task<TestSessionDetailDto?> GetSessionDetailAsync(int userId, int sessionId);
}
