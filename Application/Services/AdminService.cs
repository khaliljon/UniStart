using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Application.Services;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class AdminService : IAdminService
{
    private readonly UniStartDbContext _db;
    private readonly ILogger<AdminService> _logger;

    public AdminService(UniStartDbContext db, ILogger<AdminService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════
    //  LIST
    // ═══════════════════════════════════════════════════════

    public async Task<List<QuestionListDto>> GetQuestionsAsync(
        string? examTypeCode = null, string? topicName = null, string? difficulty = null)
    {
        var query = _db.Questions
            .Include(q => q.Topic)
                .ThenInclude(t => t.Section!)
                .ThenInclude(s => s.ExamType)
            .Include(q => q.AnswerOptions)
            .AsQueryable();

        if (!string.IsNullOrEmpty(examTypeCode))
            query = query.Where(q => q.Topic.Section!.ExamTypeCode == examTypeCode);

        if (!string.IsNullOrEmpty(topicName))
            query = query.Where(q => q.Topic.Name.Contains(topicName));

        if (!string.IsNullOrEmpty(difficulty) && Enum.TryParse<QuestionDifficulty>(difficulty, true, out var diff))
            query = query.Where(q => q.Difficulty == diff);

        var questions = await query.OrderBy(q => q.Topic.Section!.ExamTypeCode)
            .ThenBy(q => q.Topic.Name)
            .ThenBy(q => q.Difficulty)
            .ToListAsync();

        return questions.Select(q => new QuestionListDto(
            Id: q.Id,
            TopicName: q.Topic.Name,
            SectionName: q.Topic.Section?.Name ?? "",
            ExamTypeCode: q.Topic.Section?.ExamTypeCode ?? "",
            Text: q.Text.Length > 120 ? q.Text[..120] + "…" : q.Text,
            Difficulty: q.Difficulty.ToString(),
            DifficultyParam: Math.Round(q.DifficultyParam, 2),
            DiscriminationParam: Math.Round(q.DiscriminationParam, 2),
            AnswerCount: q.AnswerOptions.Count,
            CreatedAt: q.CreatedAt
        )).ToList();
    }

    // ═══════════════════════════════════════════════════════
    //  GET BY ID
    // ═══════════════════════════════════════════════════════

    public async Task<QuestionDetailDto?> GetQuestionByIdAsync(int id)
    {
        var q = await _db.Questions
            .Include(q => q.Topic)
                .ThenInclude(t => t.Section!)
                .ThenInclude(s => s.ExamType)
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (q == null) return null;
        return MapDetail(q);
    }

    // ═══════════════════════════════════════════════════════
    //  CREATE
    // ═══════════════════════════════════════════════════════

    public async Task<QuestionDetailDto> CreateQuestionAsync(CreateQuestionDto dto)
    {
        var topic = await _db.Topics
            .Include(t => t.Section!)
            .ThenInclude(s => s.ExamType)
            .FirstOrDefaultAsync(t => t.Id == dto.TopicId);

        if (topic == null)
            throw new ArgumentException($"Topic with Id {dto.TopicId} not found");

        if (!Enum.TryParse<QuestionDifficulty>(dto.Difficulty, true, out var difficulty))
            throw new ArgumentException($"Invalid difficulty: {dto.Difficulty}. Use Easy, Medium, or Hard.");

        if (dto.AnswerOptions == null || dto.AnswerOptions.Count < 2)
            throw new ArgumentException("At least 2 answer options required");

        if (!dto.AnswerOptions.Any(o => o.IsCorrect))
            throw new ArgumentException("At least one answer must be marked correct");

        var question = new Question
        {
            TopicId = dto.TopicId,
            Text = dto.Text,
            Difficulty = difficulty,
            Explanation = dto.Explanation,
            DifficultyParam = dto.DifficultyParam ?? IrtMath.DifficultyToParam(difficulty),
            DiscriminationParam = dto.DiscriminationParam ?? IrtMath.DifficultyToDiscrimination(difficulty),
            GuessParam = dto.GuessParam ?? (1.0 / dto.AnswerOptions.Count),
            CreatedAt = DateTime.UtcNow,
            AnswerOptions = dto.AnswerOptions.Select(o => new AnswerOption
            {
                Text = o.Text,
                IsCorrect = o.IsCorrect
            }).ToList()
        };

        _db.Questions.Add(question);
        await _db.SaveChangesAsync();

        // Reload with navigation
        return (await GetQuestionByIdAsync(question.Id))!;
    }

    // ═══════════════════════════════════════════════════════
    //  UPDATE
    // ═══════════════════════════════════════════════════════

    public async Task<QuestionDetailDto?> UpdateQuestionAsync(int id, UpdateQuestionDto dto)
    {
        var question = await _db.Questions
            .Include(q => q.AnswerOptions)
            .Include(q => q.Topic)
                .ThenInclude(t => t.Section!)
                .ThenInclude(s => s.ExamType)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null) return null;

        if (dto.Text != null) question.Text = dto.Text;
        if (dto.Explanation != null) question.Explanation = dto.Explanation;
        if (dto.DifficultyParam.HasValue) question.DifficultyParam = dto.DifficultyParam.Value;
        if (dto.DiscriminationParam.HasValue) question.DiscriminationParam = dto.DiscriminationParam.Value;
        if (dto.GuessParam.HasValue) question.GuessParam = dto.GuessParam.Value;

        if (dto.Difficulty != null && Enum.TryParse<QuestionDifficulty>(dto.Difficulty, true, out var diff))
            question.Difficulty = diff;

        if (dto.AnswerOptions != null && dto.AnswerOptions.Count >= 2)
        {
            _db.AnswerOptions.RemoveRange(question.AnswerOptions);
            question.AnswerOptions = dto.AnswerOptions.Select(o => new AnswerOption
            {
                Text = o.Text,
                IsCorrect = o.IsCorrect
            }).ToList();
        }

        await _db.SaveChangesAsync();
        return MapDetail(question);
    }

    // ═══════════════════════════════════════════════════════
    //  DELETE
    // ═══════════════════════════════════════════════════════

    public async Task<bool> DeleteQuestionAsync(int id)
    {
        var question = await _db.Questions
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null) return false;

        // Remove related user answers first
        var userAnswers = await _db.UserAnswers.Where(a => a.QuestionId == id).ToListAsync();
        _db.UserAnswers.RemoveRange(userAnswers);

        _db.AnswerOptions.RemoveRange(question.AnswerOptions);
        _db.Questions.Remove(question);
        await _db.SaveChangesAsync();

        return true;
    }

    // ═══════════════════════════════════════════════════════
    //  BULK IMPORT
    // ═══════════════════════════════════════════════════════

    public async Task<BulkImportResultDto> BulkImportAsync(BulkImportDto dto)
    {
        var errors = new List<string>();
        var imported = 0;

        var topicIdsList = await _db.Topics.Select(t => t.Id).ToListAsync();
        var topicIds = topicIdsList.ToHashSet();

        for (int i = 0; i < dto.Questions.Count; i++)
        {
            var q = dto.Questions[i];
            try
            {
                if (!topicIds.Contains(q.TopicId))
                {
                    errors.Add($"[{i}] Topic {q.TopicId} not found");
                    continue;
                }

                if (q.AnswerOptions == null || q.AnswerOptions.Count < 2)
                {
                    errors.Add($"[{i}] Need at least 2 answer options");
                    continue;
                }

                if (!q.AnswerOptions.Any(o => o.IsCorrect))
                {
                    errors.Add($"[{i}] No correct answer marked");
                    continue;
                }

                if (!Enum.TryParse<QuestionDifficulty>(q.Difficulty, true, out var diff))
                {
                    errors.Add($"[{i}] Invalid difficulty: {q.Difficulty}");
                    continue;
                }

                var question = new Question
                {
                    TopicId = q.TopicId,
                    Text = q.Text,
                    Difficulty = diff,
                    Explanation = q.Explanation,
                    DifficultyParam = q.DifficultyParam ?? IrtMath.DifficultyToParam(diff),
                    DiscriminationParam = q.DiscriminationParam ?? IrtMath.DifficultyToDiscrimination(diff),
                    GuessParam = q.GuessParam ?? 0.25,
                    CreatedAt = DateTime.UtcNow,
                    AnswerOptions = q.AnswerOptions.Select(o => new AnswerOption
                    {
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                };

                _db.Questions.Add(question);
                imported++;
            }
            catch (Exception ex)
            {
                errors.Add($"[{i}] {ex.Message}");
            }
        }

        if (imported > 0)
            await _db.SaveChangesAsync();

        _logger.LogInformation("Bulk import: {Imported}/{Total} imported, {Failed} failed",
            imported, dto.Questions.Count, errors.Count);

        return new BulkImportResultDto(
            Total: dto.Questions.Count,
            Imported: imported,
            Failed: errors.Count,
            Errors: errors
        );
    }

    // ═══════════════════════════════════════════════════════
    //  STATS
    // ═══════════════════════════════════════════════════════

    public async Task<QuestionStatsDto> GetStatsAsync()
    {
        var questions = await _db.Questions
            .Include(q => q.Topic)
                .ThenInclude(t => t.Section!)
            .ToListAsync();

        var allTopics = await _db.Topics.CountAsync();
        var topicsWithQ = questions.Select(q => q.TopicId).Distinct().Count();

        return new QuestionStatsDto(
            TotalQuestions: questions.Count,
            ByExam: questions.GroupBy(q => q.Topic.Section?.ExamTypeCode ?? "?")
                .ToDictionary(g => g.Key, g => g.Count()),
            ByDifficulty: questions.GroupBy(q => q.Difficulty.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            ByTopic: questions.GroupBy(q => q.Topic.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            TopicsWithQuestions: topicsWithQ,
            TopicsWithoutQuestions: allTopics - topicsWithQ
        );
    }

    // ═══════════════════════════════════════════════════════
    //  MAP
    // ═══════════════════════════════════════════════════════

    private static QuestionDetailDto MapDetail(Question q) => new(
        Id: q.Id,
        TopicId: q.TopicId,
        TopicName: q.Topic.Name,
        SectionName: q.Topic.Section?.Name ?? "",
        ExamTypeCode: q.Topic.Section?.ExamTypeCode ?? "",
        Text: q.Text,
        Difficulty: q.Difficulty.ToString(),
        Explanation: q.Explanation,
        DifficultyParam: Math.Round(q.DifficultyParam, 3),
        DiscriminationParam: Math.Round(q.DiscriminationParam, 3),
        GuessParam: Math.Round(q.GuessParam, 3),
        CreatedAt: q.CreatedAt,
        AnswerOptions: q.AnswerOptions.Select(o => new AdminAnswerOptionDto(o.Id, o.Text, o.IsCorrect)).ToList()
    );

    // ═══════════════════════════════════════════════════════
    //  USERS — LIST
    // ═══════════════════════════════════════════════════════

    public async Task<List<AdminUserDto>> GetUsersAsync(string? role = null, string? search = null)
    {
        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var r))
            query = query.Where(u => u.Role == r);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.Email.Contains(search) || u.Name.Contains(search));

        var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

        var userIds = users.Select(u => u.Id).ToList();

        var answerStats = await _db.UserAnswers
            .Include(a => a.AnswerOption)
            .Where(a => userIds.Contains(a.UserId))
            .GroupBy(a => a.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Total = g.Count(),
                Correct = g.Count(a => a.AnswerOption.IsCorrect)
            })
            .ToDictionaryAsync(x => x.UserId);

        var sessionCounts = await _db.TestSessions
            .Where(s => userIds.Contains(s.UserId))
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        return users.Select(u =>
        {
            answerStats.TryGetValue(u.Id, out var stats);
            sessionCounts.TryGetValue(u.Id, out var sessions);

            return new AdminUserDto(
                Id: u.Id,
                Email: u.Email,
                Name: u.Name,
                Role: u.Role.ToString(),
                SubscriptionTier: u.SubscriptionTier.ToString(),
                SubscriptionExpiresAt: u.SubscriptionExpiresAt,
                HasCompletedOnboarding: u.HasCompletedOnboarding,
                CreatedAt: u.CreatedAt,
                UpdatedAt: u.UpdatedAt,
                TotalAnswers: stats?.Total ?? 0,
                CorrectAnswers: stats?.Correct ?? 0,
                TestSessions: sessions
            );
        }).ToList();
    }

    // ═══════════════════════════════════════════════════════
    //  USERS — GET BY ID
    // ═══════════════════════════════════════════════════════

    public async Task<AdminUserDto?> GetUserByIdAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return null;

        var totalAnswers = await _db.UserAnswers.CountAsync(a => a.UserId == id);
        var correctAnswers = await _db.UserAnswers
            .Include(a => a.AnswerOption)
            .CountAsync(a => a.UserId == id && a.AnswerOption.IsCorrect);
        var testSessions = await _db.TestSessions.CountAsync(s => s.UserId == id);

        return new AdminUserDto(
            Id: user.Id,
            Email: user.Email,
            Name: user.Name,
            Role: user.Role.ToString(),
            SubscriptionTier: user.SubscriptionTier.ToString(),
            SubscriptionExpiresAt: user.SubscriptionExpiresAt,
            HasCompletedOnboarding: user.HasCompletedOnboarding,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt,
            TotalAnswers: totalAnswers,
            CorrectAnswers: correctAnswers,
            TestSessions: testSessions
        );
    }

    // ═══════════════════════════════════════════════════════
    //  USERS — UPDATE
    // ═══════════════════════════════════════════════════════

    public async Task<AdminUserDto?> UpdateUserAsync(int id, AdminUpdateUserDto dto)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            user.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailTaken = await _db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id);
            if (emailTaken)
                throw new InvalidOperationException("Email is already taken");
            user.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.Role) && Enum.TryParse<UserRole>(dto.Role, true, out var role))
            user.Role = role;

        if (!string.IsNullOrWhiteSpace(dto.SubscriptionTier) && Enum.TryParse<SubscriptionTier>(dto.SubscriptionTier, true, out var tier))
            user.SubscriptionTier = tier;

        if (dto.SubscriptionExpiresAt.HasValue)
            user.SubscriptionExpiresAt = dto.SubscriptionExpiresAt.Value;

        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GetUserByIdAsync(id);
    }

    // ═══════════════════════════════════════════════════════
    //  USERS — DELETE
    // ═══════════════════════════════════════════════════════

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;

        // Prevent deleting the last admin
        if (user.Role == UserRole.Admin)
        {
            var adminCount = await _db.Users.CountAsync(u => u.Role == UserRole.Admin);
            if (adminCount <= 1)
                throw new InvalidOperationException("Cannot delete the last admin user");
        }

        // Remove related data
        var userAnswers = await _db.UserAnswers.Where(a => a.UserId == id).ToListAsync();
        _db.UserAnswers.RemoveRange(userAnswers);

        var skillProfiles = await _db.UserSkillProfiles.Where(sp => sp.UserId == id).ToListAsync();
        _db.UserSkillProfiles.RemoveRange(skillProfiles);

        var testSessions = await _db.TestSessions.Where(s => s.UserId == id).ToListAsync();
        _db.TestSessions.RemoveRange(testSessions);

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Admin deleted user {UserId} ({Email})", id, user.Email);
        return true;
    }

    // ═══════════════════════════════════════════════════════
    //  USERS — STATS
    // ═══════════════════════════════════════════════════════

    public async Task<AdminUserStatsDto> GetUserStatsAsync()
    {
        var users = await _db.Users.ToListAsync();
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var activeUserIds = await _db.UserAnswers
            .Where(a => a.AnsweredAt >= sevenDaysAgo)
            .Select(a => a.UserId)
            .Distinct()
            .CountAsync();

        return new AdminUserStatsDto(
            TotalUsers: users.Count,
            Students: users.Count(u => u.Role == UserRole.Student),
            Tutors: users.Count(u => u.Role == UserRole.Tutor),
            Admins: users.Count(u => u.Role == UserRole.Admin),
            ProUsers: users.Count(u => u.SubscriptionTier == SubscriptionTier.Pro),
            ActiveLast7Days: activeUserIds
        );
    }

    // ═══════════════════════════════════════════════════════
    //  DASHBOARD
    // ═══════════════════════════════════════════════════════

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var questionStats = await GetStatsAsync();
        var userStats = await GetUserStatsAsync();
        var topics = await GetTopicsAsync();

        return new AdminDashboardDto(
            QuestionStats: questionStats,
            UserStats: userStats,
            Topics: topics
        );
    }

    // ═══════════════════════════════════════════════════════
    //  TOPICS LIST
    // ═══════════════════════════════════════════════════════

    public async Task<List<AdminTopicSummaryDto>> GetTopicsAsync()
    {
        var topics = await _db.Topics
            .Include(t => t.Section!)
            .Include(t => t.Questions)
            .OrderBy(t => t.Section!.ExamTypeCode)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return topics.Select(t => new AdminTopicSummaryDto(
            Id: t.Id,
            Name: t.Name,
            SectionName: t.Section?.Name ?? "",
            ExamTypeCode: t.Section?.ExamTypeCode ?? "",
            QuestionCount: t.Questions.Count
        )).ToList();
    }
}
