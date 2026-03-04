using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(15);

    public AdminService(UniStartDbContext db, ILogger<AdminService> logger, IMemoryCache cache)
    {
        _db = db;
        _logger = logger;
        _cache = cache;
    }

    // ═══════════════════════════════════════════════════════
    //  LIST
    // ═══════════════════════════════════════════════════════

    public async Task<PagedResult<QuestionListDto>> GetQuestionsAsync(
        string? examTypeCode = null, string? topicName = null, string? difficulty = null,
        int page = 1, int pageSize = 50)
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

        var orderedQuery = query.OrderBy(q => q.Topic.Section!.ExamTypeCode)
            .ThenBy(q => q.Topic.Name)
            .ThenBy(q => q.Difficulty);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var totalCount = await orderedQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var questions = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = questions.Select(q => new QuestionListDto(
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

        return new PagedResult<QuestionListDto>(items, totalCount, page, pageSize, totalPages);
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

        // Soft delete (OP-9) — mark as deleted instead of removing
        question.IsDeleted = true;
        question.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Soft-deleted question {QuestionId}", id);
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

        var allTopics = await _db.Topics.Include(t => t.Section).ToListAsync();
        var topicIdsWithQ = questions.Select(q => q.TopicId).Distinct().ToHashSet();
        var uncoveredTopics = allTopics
            .Where(t => !topicIdsWithQ.Contains(t.Id))
            .Select(t => $"{t.Section?.ExamTypeCode ?? "?"}: {t.Name}")
            .OrderBy(n => n)
            .ToList();

        return new QuestionStatsDto(
            TotalQuestions: questions.Count,
            ByExam: questions.GroupBy(q => q.Topic.Section?.ExamTypeCode ?? "?")
                .ToDictionary(g => g.Key, g => g.Count()),
            ByDifficulty: questions.GroupBy(q => q.Difficulty.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            ByTopic: questions.GroupBy(q => q.Topic.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            TopicsWithQuestions: topicIdsWithQ.Count,
            TopicsWithoutQuestions: uncoveredTopics.Count,
            TopicsWithoutQuestionsList: uncoveredTopics
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

    public async Task<PagedResult<AdminUserDto>> GetUsersAsync(string? role = null, string? search = null,
        int page = 1, int pageSize = 50)
    {
        var query = _db.Users.AsQueryable();

        if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var r))
            query = query.Where(u => u.Role == r);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.Email.Contains(search) || u.Name.Contains(search));

        var orderedQuery = query.OrderByDescending(u => u.CreatedAt);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var totalCount = await orderedQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var users = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

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

        var items = users.Select(u =>
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
                IsBlocked: u.IsBlocked,
                BlockedAt: u.BlockedAt,
                BlockReason: u.BlockReason,
                CreatedAt: u.CreatedAt,
                UpdatedAt: u.UpdatedAt,
                TotalAnswers: stats?.Total ?? 0,
                CorrectAnswers: stats?.Correct ?? 0,
                TestSessions: sessions
            );
        }).ToList();

        return new PagedResult<AdminUserDto>(items, totalCount, page, pageSize, totalPages);
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
            IsBlocked: user.IsBlocked,
            BlockedAt: user.BlockedAt,
            BlockReason: user.BlockReason,
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

        // Soft delete (OP-9) — mark as deleted instead of removing
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Soft-deleted user {UserId} ({Email})", id, user.Email);
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

    // ═══════════════════════════════════════════════════════
    //  CREATE TOPIC
    // ═══════════════════════════════════════════════════════

    public async Task<AdminTopicSummaryDto> CreateTopicAsync(CreateTopicDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Название темы не может быть пустым");

        var section = await _db.ExamSections.FindAsync(dto.SectionId)
            ?? throw new ArgumentException($"Секция с ID {dto.SectionId} не найдена");

        var skill = await _db.Skills.FindAsync(dto.SkillId)
            ?? throw new ArgumentException($"Навык с ID {dto.SkillId} не найден");

        var exists = await _db.Topics.AnyAsync(t => t.Name == dto.Name && t.SectionId == dto.SectionId);
        if (exists)
            throw new ArgumentException($"Тема '{dto.Name}' уже существует в секции '{section.Name}'");

        var topic = new Topic
        {
            Name = dto.Name,
            SectionId = dto.SectionId,
            SkillId = dto.SkillId
        };

        _db.Topics.Add(topic);
        await _db.SaveChangesAsync();

        // Invalidate caches
        _cache.Remove("admin:sections");
        _cache.Remove("admin:skills");

        return new AdminTopicSummaryDto(
            Id: topic.Id,
            Name: topic.Name,
            SectionName: section.Name,
            ExamTypeCode: section.ExamTypeCode,
            QuestionCount: 0
        );
    }

    // ═══════════════════════════════════════════════════════
    //  SECTIONS & SKILLS (for dropdowns)
    // ═══════════════════════════════════════════════════════

    public async Task<List<AdminSectionDto>> GetSectionsAsync()
    {
        return await _cache.GetOrCreateAsync("admin:sections", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await _db.ExamSections
                .OrderBy(s => s.ExamTypeCode)
                .ThenBy(s => s.Name)
                .Select(s => new AdminSectionDto(s.Id, s.Name, s.ExamTypeCode))
                .ToListAsync();
        }) ?? [];
    }

    public async Task<List<AdminSkillDto>> GetSkillsAsync()
    {
        return await _cache.GetOrCreateAsync("admin:skills", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return await _db.Skills
                .OrderBy(s => s.Name)
                .Select(s => new AdminSkillDto(s.Id, s.Code, s.Name))
                .ToListAsync();
        }) ?? [];
    }

    // ═══════════════════════════════════════════════════════
    //  RESTORE (Soft Delete — OP-9)
    // ═══════════════════════════════════════════════════════

    public async Task<bool> RestoreQuestionAsync(int id)
    {
        // IgnoreQueryFilters to find soft-deleted records
        var question = await _db.Questions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(q => q.Id == id && q.IsDeleted);

        if (question == null) return false;

        question.IsDeleted = false;
        question.DeletedAt = null;
        question.DeletedBy = null;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Restored question {QuestionId}", id);
        return true;
    }

    public async Task<bool> RestoreUserAsync(int id)
    {
        var user = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted);

        if (user == null) return false;

        user.IsDeleted = false;
        user.DeletedAt = null;
        user.DeletedBy = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Restored user {UserId} ({Email})", id, user.Email);
        return true;
    }

    // ═══════════════════════════════════════════════════════
    //  BLOCK / SUSPEND (OP-14)
    // ═══════════════════════════════════════════════════════

    public async Task<AdminUserDto?> BlockUserAsync(int id, string? reason = null)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return null;

        if (user.Role == UserRole.Admin)
            throw new InvalidOperationException("Cannot block an admin user");

        user.IsBlocked = true;
        user.BlockedAt = DateTime.UtcNow;
        user.BlockReason = reason;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Blocked user {UserId} ({Email}). Reason: {Reason}", id, user.Email, reason ?? "none");
        return await GetUserByIdAsync(id);
    }

    public async Task<AdminUserDto?> UnblockUserAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return null;

        user.IsBlocked = false;
        user.BlockedAt = null;
        user.BlockReason = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Unblocked user {UserId} ({Email})", id, user.Email);
        return await GetUserByIdAsync(id);
    }
}
