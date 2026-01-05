using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Repositories;

/// <summary>
/// Repository implementation for Achievement entity
/// </summary>
public class AchievementRepository : Repository<Achievement>, IAchievementRepository
{
    public AchievementRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Achievement>> GetAchievementsByTypeAsync(string type)
    {
        return await _dbSet
            .Where(a => a.Type == type)
            .OrderBy(a => a.TargetValue)
            .ToListAsync();
    }

    public async Task<IEnumerable<Achievement>> GetUserAchievementsAsync(string userId)
    {
        return await _dbSet
            .Where(a => a.UserAchievements.Any(ua => ua.UserId == userId && ua.IsCompleted))
            .ToListAsync();
    }

    public async Task<int> GetUnlockedCountAsync(string userId)
    {
        return await _context.UserAchievements
            .Where(ua => ua.UserId == userId && ua.IsCompleted)
            .CountAsync();
    }
}
