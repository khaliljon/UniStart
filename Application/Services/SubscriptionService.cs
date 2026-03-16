using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Domain.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly UniStartDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    // ── Tier configuration ──

    private static readonly Dictionary<string, TierLimitsDto> TierConfigs = new()
    {
        ["Free"] = new TierLimitsDto(
            QuestionsPerDay: 15,
            LessonsPerDay: 1,
            MockExamsEnabled: false,
            FullAnalytics: false,
            FullStudyPlan: false,
            RealtimePrediction: false
        ),
        ["Pro"] = new TierLimitsDto(
            QuestionsPerDay: int.MaxValue,
            LessonsPerDay: int.MaxValue,
            MockExamsEnabled: true,
            FullAnalytics: true,
            FullStudyPlan: true,
            RealtimePrediction: true
        ),
    };

    public SubscriptionService(UniStartDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubscriptionStatusDto> GetStatusAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        var tier = ResolveTier(user);
        var limits = GetLimits(tier);
        var usage = await GetDailyUsageAsync(userId);

        return new SubscriptionStatusDto(
            Tier: tier,
            IsPro: user.IsPro,
            ExpiresAt: user.SubscriptionExpiresAt,
            DailyUsage: usage,
            Limits: limits,
            FreeMockAvailable: !user.IsPro && !user.FreeMockUsed
        );
    }

    public async Task<DailyUsageDto> GetDailyUsageAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        var tier = ResolveTier(user);
        var limits = GetLimits(tier);

        var todayUtc = DateTime.UtcNow.Date;

        // Count answers today (excluding diagnostic placeholders with TimeSpentSeconds == -1)
        var questionsToday = await _context.UserAnswers
            .CountAsync(a => a.UserId == userId
                          && a.AnsweredAt >= todayUtc
                          && a.TimeSpentSeconds >= 0);

        // Count unique lessons viewed today — approximate via UserAnswers on unique topics
        // For a proper implementation we'd have a LessonView table, but for now use a simple count
        var lessonsToday = 0; // TODO: implement when LessonView tracking exists

        var questionsRemaining = Math.Max(0, limits.QuestionsPerDay - questionsToday);

        return new DailyUsageDto(
            QuestionsAnswered: questionsToday,
            QuestionsLimit: limits.QuestionsPerDay == int.MaxValue ? -1 : limits.QuestionsPerDay,
            QuestionsRemaining: limits.QuestionsPerDay == int.MaxValue ? -1 : questionsRemaining,
            LessonsViewed: lessonsToday,
            LessonsLimit: limits.LessonsPerDay == int.MaxValue ? -1 : limits.LessonsPerDay,
            LessonsRemaining: limits.LessonsPerDay == int.MaxValue ? -1 : Math.Max(0, limits.LessonsPerDay - lessonsToday),
            IsLimitReached: !user.IsPro && questionsToday >= limits.QuestionsPerDay,
            ServerTimeUtc: DateTime.UtcNow
        );
    }

    public async Task<bool> HasAccessAsync(int userId, string feature)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        if (user.IsPro || user.Role == UserRole.Admin) return true;

        var tier = ResolveTier(user);
        var limits = GetLimits(tier);

        return feature.ToLower() switch
        {
            "questions" => true,          // with daily limit
            "basic_analytics" => true,
            "weekly_prediction" => true,
            "study_plan_basic" => true,
            "mock_exams" => limits.MockExamsEnabled || !user.FreeMockUsed,
            "full_analytics" => limits.FullAnalytics,
            "full_study_plan" => limits.FullStudyPlan,
            "realtime_prediction" => limits.RealtimePrediction,
            _ => false
        };
    }

    public async Task<bool> CanAnswerQuestionAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;
        if (user.IsPro || user.Role == UserRole.Admin) return true;

        var tier = ResolveTier(user);
        var limits = GetLimits(tier);
        var todayUtc = DateTime.UtcNow.Date;

        var questionsToday = await _context.UserAnswers
            .CountAsync(a => a.UserId == userId
                          && a.AnsweredAt >= todayUtc
                          && a.TimeSpentSeconds >= 0);

        return questionsToday < limits.QuestionsPerDay;
    }

    public async Task<UpgradeResponseDto> UpgradeAsync(int userId, UpgradeRequestDto dto)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        // Stub: in production this would integrate with a payment provider (Kaspi, etc.)
        if (dto.Plan.Equals("Pro", StringComparison.OrdinalIgnoreCase))
        {
            user.SubscriptionTier = SubscriptionTier.Pro;
            user.SubscriptionExpiresAt = DateTime.UtcNow.AddDays(30);
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return new UpgradeResponseDto(
                Success: true,
                Tier: "Pro",
                ExpiresAt: user.SubscriptionExpiresAt,
                Message: "Вы успешно перешли на тариф Pro (1 месяц)! Все функции разблокированы."
            );
        }

        if (dto.Plan.Equals("ProYearly", StringComparison.OrdinalIgnoreCase))
        {
            user.SubscriptionTier = SubscriptionTier.Pro;
            user.SubscriptionExpiresAt = DateTime.UtcNow.AddDays(365);
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return new UpgradeResponseDto(
                Success: true,
                Tier: "Pro",
                ExpiresAt: user.SubscriptionExpiresAt,
                Message: "Вы успешно перешли на тариф Pro (1 год)! Все функции разблокированы."
            );
        }

        // Downgrade to Free
        user.SubscriptionTier = SubscriptionTier.Free;
        user.SubscriptionExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return new UpgradeResponseDto(
            Success: true,
            Tier: "Free",
            ExpiresAt: null,
            Message: "Вы перешли на бесплатный тариф."
        );
    }

    public TierLimitsDto GetLimits(string tier)
    {
        return TierConfigs.TryGetValue(tier, out var config)
            ? config
            : TierConfigs["Free"];
    }

    private static string ResolveTier(User user)
    {
        return user.IsPro ? "Pro" : "Free";
    }
}
