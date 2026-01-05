namespace UniStart.Services;

/// <summary>
/// Service for platform statistics and analytics
/// </summary>
public interface IStatisticsService
{
    Task<Dictionary<string, object>> GetPlatformStatsAsync();
    Task<Dictionary<string, object>> GetUserStatsAsync(string userId);
    Task<Dictionary<string, object>> GetDailyActivityAsync(int days);
    Task<Dictionary<string, object>> GetHourlyActivityAsync();
    Task<IEnumerable<object>> GetTopUsersAsync(int count);
    Task<IEnumerable<object>> GetPopularSubjectsAsync(int count);
    Task<IEnumerable<object>> GetPopularQuizzesAsync(int count);
}
