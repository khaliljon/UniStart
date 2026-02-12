using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly UniStartDbContext _context;

    public AnalyticsService(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task<SkillAnalyticsDto> GetUserAnalyticsAsync(int userId)
    {
        // Get total questions answered
        var userAnswers = await _context.UserAnswers
            .Include(ua => ua.AnswerOption)
            .Where(ua => ua.UserId == userId)
            .ToListAsync();

        var totalQuestionsAnswered = userAnswers.Count;
        var correctAnswers = userAnswers.Count(ua => ua.AnswerOption.IsCorrect);
        var overallAccuracy = totalQuestionsAnswered > 0 
            ? (double)correctAnswers / totalQuestionsAnswered * 100 
            : 0;

        // Get skill profiles
        var skillProfiles = await GetUserSkillsAsync(userId);

        // Get recent progress (last 10 answers with skill changes)
        var recentProgress = await GetRecentProgressAsync(userId);

        return new SkillAnalyticsDto(
            totalQuestionsAnswered,
            correctAnswers,
            Math.Round(overallAccuracy, 2),
            skillProfiles,
            recentProgress
        );
    }

    public async Task<IEnumerable<UserSkillProfileDto>> GetUserSkillsAsync(int userId)
    {
        var profiles = await _context.UserSkillProfiles
            .Include(p => p.Skill)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.Level)
            .ToListAsync();

        return profiles.Select(p => new UserSkillProfileDto(
            p.SkillId,
            p.Skill.Name,
            p.Skill.Code,
            p.Level,
            p.LastUpdated
        ));
    }

    private async Task<IEnumerable<SkillProgressDto>> GetRecentProgressAsync(int userId)
    {
        // Get skills that have been updated recently
        var recentUpdates = await _context.UserSkillProfiles
            .Include(p => p.Skill)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.LastUpdated)
            .Take(10)
            .Select(p => new SkillProgressDto(
                p.Skill.Name,
                p.Level - 5, // Approximate previous level (simplified)
                p.Level,
                p.LastUpdated
            ))
            .ToListAsync();

        return recentUpdates;
    }
}
