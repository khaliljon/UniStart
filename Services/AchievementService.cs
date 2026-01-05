using Microsoft.EntityFrameworkCore;
using UniStart.DTOs;
using UniStart.Models;
using UniStart.Repositories;

namespace UniStart.Services;

/// <summary>
/// Service implementation for Achievement business logic
/// </summary>
public class AchievementService : IAchievementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AchievementService> _logger;

    public AchievementService(IUnitOfWork unitOfWork, ILogger<AchievementService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<Achievement>> GetAllAchievementsAsync()
    {
        return await _unitOfWork.Achievements.GetAllAsync();
    }

    public async Task<IEnumerable<Achievement>> GetUserAchievementsAsync(string userId)
    {
        return await _unitOfWork.Achievements.GetUserAchievementsAsync(userId);
    }

    public async Task<IEnumerable<Achievement>> GetUnlockedAchievementsAsync(string userId)
    {
        var userAchievements = await _unitOfWork.Repository<UserAchievement>()
            .FindAsync(ua => ua.UserId == userId && ua.IsCompleted);

        var achievementIds = userAchievements.Select(ua => ua.AchievementId).ToList();
        
        return await _unitOfWork.Achievements.FindAsync(a => achievementIds.Contains(a.Id));
    }

    public async Task CheckAndUnlockAchievementsAsync(string userId)
    {
        var allAchievements = await _unitOfWork.Achievements.GetAllAsync();
        var userAchievements = await _unitOfWork.Repository<UserAchievement>()
            .FindAsync(ua => ua.UserId == userId);

        var userAchievementDict = userAchievements.ToDictionary(ua => ua.AchievementId);

        // Получаем статистику пользователя
        var quizAttempts = await _unitOfWork.QuizAttempts.CountAsync(qa => qa.UserId == userId);
        var examAttempts = await _unitOfWork.ExamAttempts.CountAsync(ea => ea.UserId == userId);
        var masteredCards = await _unitOfWork.FlashcardProgress.CountAsync(p => p.UserId == userId && p.IsMastered);

        foreach (var achievement in allAchievements)
        {
            if (userAchievementDict.ContainsKey(achievement.Id) && userAchievementDict[achievement.Id].IsCompleted)
                continue; // Уже разблокировано

            int currentProgress = achievement.Type switch
            {
                "QuizCompleted" => quizAttempts,
                "ExamCompleted" => examAttempts,
                "FlashcardMastered" => masteredCards,
                _ => 0
            };

            if (currentProgress >= achievement.TargetValue)
            {
                await UnlockAchievementAsync(userId, achievement.Id);
            }
            else if (userAchievementDict.ContainsKey(achievement.Id))
            {
                // Обновляем прогресс
                var userAchievement = userAchievementDict[achievement.Id];
                userAchievement.Progress = currentProgress;
                _unitOfWork.Repository<UserAchievement>().Update(userAchievement);
            }
            else
            {
                // Создаем новую запись прогресса
                var newUserAchievement = new UserAchievement
                {
                    UserId = userId,
                    AchievementId = achievement.Id,
                    Progress = currentProgress,
                    IsCompleted = false
                };
                await _unitOfWork.Repository<UserAchievement>().AddAsync(newUserAchievement);
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> UnlockAchievementAsync(string userId, int achievementId)
    {
        var userAchievement = await _unitOfWork.Repository<UserAchievement>()
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);

        if (userAchievement == null)
        {
            var achievement = await _unitOfWork.Achievements.GetByIdAsync(achievementId);
            if (achievement == null)
                return false;

            userAchievement = new UserAchievement
            {
                UserId = userId,
                AchievementId = achievementId,
                Progress = achievement.TargetValue,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<UserAchievement>().AddAsync(userAchievement);
        }
        else if (!userAchievement.IsCompleted)
        {
            userAchievement.IsCompleted = true;
            userAchievement.CompletedAt = DateTime.UtcNow;
            _unitOfWork.Repository<UserAchievement>().Update(userAchievement);
        }
        else
        {
            return false; // Уже разблокировано
        }

        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Achievement {AchievementId} unlocked for user {UserId}", achievementId, userId);

        return true;
    }

    public async Task<Dictionary<string, int>> GetUserAchievementStatsAsync(string userId)
    {
        var totalAchievements = await _unitOfWork.Achievements.CountAsync();
        var unlockedCount = await _unitOfWork.Achievements.GetUnlockedCountAsync(userId);

        var userAchievements = await _unitOfWork.Repository<UserAchievement>()
            .FindAsync(ua => ua.UserId == userId);

        var inProgress = userAchievements.Count(ua => !ua.IsCompleted && ua.Progress > 0);

        return new Dictionary<string, int>
        {
            ["total"] = totalAchievements,
            ["unlocked"] = unlockedCount,
            ["inProgress"] = inProgress,
            ["locked"] = totalAchievements - unlockedCount - inProgress
        };
    }
}
