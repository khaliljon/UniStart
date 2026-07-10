using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Application.Services;
using UniStart.Application.Helpers;
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
        string? sectionName = null, int page = 1, int pageSize = 50)
    {
        var query = _db.Questions
            .Include(q => q.Topic)
                .ThenInclude(t => t.Section!)
                .ThenInclude(s => s.ExamType)
            .Include(q => q.AnswerOptions)
            .AsQueryable();

        if (!string.IsNullOrEmpty(examTypeCode))
            query = query.Where(q => q.Topic.Section!.ExamTypeCode == examTypeCode);

        if (!string.IsNullOrEmpty(sectionName))
            query = query.Where(q => q.Topic.Section!.Name == sectionName);

        if (!string.IsNullOrEmpty(topicName))
            query = query.Where(q => q.Topic.Name.Contains(topicName));

        if (!string.IsNullOrEmpty(difficulty) && Enum.TryParse<QuestionDifficulty>(difficulty, true, out var diff))
            query = query.Where(q => q.Difficulty == diff);

        var orderedQuery = query.OrderBy(q => q.Topic.Section!.ExamTypeCode)
            .ThenBy(q => q.Topic.SectionId)
            .ThenBy(q => q.TopicId)
            .ThenBy(q => q.Difficulty);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 1000);

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
            ImageUrl = dto.ImageUrl,
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

        if (dto.TopicId.HasValue)
        {
            var topicExists = await _db.Topics.AnyAsync(t => t.Id == dto.TopicId.Value);
            if (!topicExists) throw new ArgumentException($"Topic with ID {dto.TopicId.Value} not found");
            question.TopicId = dto.TopicId.Value;
        }

        if (dto.Text != null) question.Text = dto.Text;
        if (dto.Explanation != null) question.Explanation = dto.Explanation;
        if (dto.ImageUrl != null) question.ImageUrl = dto.ImageUrl == string.Empty ? null : dto.ImageUrl;
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
        ImageUrl: q.ImageUrl,
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
        int page = 1, int pageSize = 50, bool includeDeleted = false)
    {
        var query = includeDeleted
            ? _db.Users.IgnoreQueryFilters()
            : _db.Users.AsQueryable();

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

        var schoolNames = new Dictionary<int, string>();

        var items = users.Select(u =>
        {
            answerStats.TryGetValue(u.Id, out var stats);
            sessionCounts.TryGetValue(u.Id, out var sessions);
            schoolNames.TryGetValue(u.SchoolId ?? 0, out var sName);

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
                IsDeleted: u.IsDeleted,
                DeletedAt: u.DeletedAt,
                CreatedAt: u.CreatedAt,
                UpdatedAt: u.UpdatedAt,
                TotalAnswers: stats?.Total ?? 0,
                CorrectAnswers: stats?.Correct ?? 0,
                TestSessions: sessions,
                SchoolId: u.SchoolId,
                SchoolName: sName,
                PhoneNumber: u.PhoneNumber
            );
        }).ToList();

        return new PagedResult<AdminUserDto>(items, totalCount, page, pageSize, totalPages);
    }

    // ═══════════════════════════════════════════════════════
    //  USERS — GET BY ID
    // ═══════════════════════════════════════════════════════

    public async Task<AdminUserDto?> GetUserByIdAsync(int id)
    {
        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return null;

        var totalAnswers = await _db.UserAnswers.CountAsync(a => a.UserId == id);
        var correctAnswers = await _db.UserAnswers
            .Include(a => a.AnswerOption)
            .CountAsync(a => a.UserId == id && a.AnswerOption.IsCorrect);
        var testSessions = await _db.TestSessions.CountAsync(s => s.UserId == id);

        // Resolve school name for display
        string? schoolName = null;

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
            IsDeleted: user.IsDeleted,
            DeletedAt: user.DeletedAt,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt,
            TotalAnswers: totalAnswers,
            CorrectAnswers: correctAnswers,
            TestSessions: testSessions,
            SchoolId: user.SchoolId,
            SchoolName: schoolName,
            PhoneNumber: user.PhoneNumber
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
            user.Name = InputSanitizer.Sanitize(dto.Name)!;

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailTaken = await _db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id);
            if (emailTaken)
                throw new InvalidOperationException("Email is already taken");
            user.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.Role) && Enum.TryParse<UserRole>(dto.Role, true, out var role))
        {
            user.Role = role;
            if (role == UserRole.Student)
                user.SchoolId = null;
        }

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
        var roleCounts = await _db.Users
            .GroupBy(u => u.Role)
            .Select(g => new { Role = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Role, x => x.Count);

        var proCount = await _db.Users.CountAsync(u => u.SubscriptionTier == SubscriptionTier.Pro);

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var activeUserIds = await _db.UserAnswers
            .Where(a => a.AnsweredAt >= sevenDaysAgo)
            .Select(a => a.UserId)
            .Distinct()
            .CountAsync();

        var total = roleCounts.Values.Sum();

        return new AdminUserStatsDto(
            TotalUsers: total,
            Students: roleCounts.GetValueOrDefault(UserRole.Student),
            Tutors: 0,
            Admins: roleCounts.GetValueOrDefault(UserRole.Admin),
            ProUsers: proCount,
            ActiveLast7Days: activeUserIds
        );
    }

    // ═══════════════════════════════════════════════════════
    //  DASHBOARD
    // ═══════════════════════════════════════════════════════

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var questionStatsTask = GetStatsAsync();
        var userStatsTask = GetUserStatsAsync();
        var topicsTask = GetTopicsAsync();

        await Task.WhenAll(questionStatsTask, userStatsTask, topicsTask);

        return new AdminDashboardDto(
            QuestionStats: questionStatsTask.Result,
            UserStats: userStatsTask.Result,
            Topics: topicsTask.Result
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
            .ThenBy(t => t.SectionId)
            .ThenBy(t => t.Id)
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

        var exists = await _db.Topics.AnyAsync(t => t.Name == dto.Name && t.SectionId == dto.SectionId);
        if (exists)
            throw new ArgumentException($"Тема '{dto.Name}' уже существует в секции '{section.Name}'");

        var topic = new Topic
        {
            Name = InputSanitizer.Sanitize(dto.Name)!,
            SectionId = dto.SectionId
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

    public async Task<AdminTopicSummaryDto?> UpdateTopicAsync(int id, UpdateTopicDto dto)
    {
        var topic = await _db.Topics
            .Include(t => t.Section!)
            .Include(t => t.Questions)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (topic == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var exists = await _db.Topics.AnyAsync(t => t.Name == dto.Name && t.SectionId == topic.SectionId && t.Id != id);
            if (exists)
                throw new ArgumentException($"Topic '{dto.Name}' already exists in this section");
            topic.Name = InputSanitizer.Sanitize(dto.Name)!;
        }

        topic.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new AdminTopicSummaryDto(
            Id: topic.Id,
            Name: topic.Name,
            SectionName: topic.Section?.Name ?? "",
            ExamTypeCode: topic.Section?.ExamTypeCode ?? "",
            QuestionCount: topic.Questions.Count
        );
    }

    public async Task<bool> DeleteTopicAsync(int id)
    {
        var topic = await _db.Topics.FindAsync(id);
        if (topic == null) return false;

        // Phase 1: clear Restrict-FK rows that block Topic→Question cascade
        // IgnoreQueryFilters to include soft-deleted questions (IsDeleted=true) —
        // they still exist in the DB and their FK dependents must be cleared too.
        var questionIds = await _db.Questions
            .IgnoreQueryFilters()
            .Where(q => q.TopicId == id)
            .Select(q => q.Id)
            .ToListAsync();

        if (questionIds.Count > 0)
        {
            // MockExamAnswer.QuestionId is Restrict
            var mockAnswers = await _db.MockExamAnswers
                .Where(a => questionIds.Contains(a.QuestionId))
                .ToListAsync();
            if (mockAnswers.Count > 0)
                _db.MockExamAnswers.RemoveRange(mockAnswers);

            // UserAnswer.AnswerOptionId is Restrict on AnswerOption (AnswerOption cascades from Question)
            var userAnswers = await _db.UserAnswers
                .Where(a => questionIds.Contains(a.QuestionId))
                .ToListAsync();
            if (userAnswers.Count > 0)
                _db.UserAnswers.RemoveRange(userAnswers);
        }

        // StudyPlanEntry.TopicId is Restrict
        var planEntries = await _db.StudyPlanEntries
            .Where(e => e.TopicId == id)
            .ToListAsync();
        if (planEntries.Count > 0)
            _db.StudyPlanEntries.RemoveRange(planEntries);

        // TopicDependency.PrerequisiteTopicId is Restrict
        var prereqDeps = await _db.TopicDependencies
            .Where(d => d.PrerequisiteTopicId == id)
            .ToListAsync();
        if (prereqDeps.Count > 0)
            _db.TopicDependencies.RemoveRange(prereqDeps);

        await _db.SaveChangesAsync(); // commit phase 1 before Topic cascades to Questions

        // Phase 2: delete topic (DB cascade deletes Questions → AnswerOptions/UserAnswers)
        _db.Topics.Remove(topic);
        await _db.SaveChangesAsync();
        _cache.Remove("admin:sections");

        _logger.LogInformation("Deleted topic {TopicId} (questions cascade-deleted)", id);
        return true;
    }

    /// <summary>
    /// Hard-delete every question of a topic (and their FK dependents) while keeping
    /// the topic itself. Returns the number of questions removed, or null if the topic
    /// does not exist.
    /// </summary>
    public async Task<int?> ClearTopicQuestionsAsync(int id)
    {
        var topic = await _db.Topics.FindAsync(id);
        if (topic == null) return null;

        // Include soft-deleted questions: they still exist in the DB and their FK
        // dependents must be cleared before the questions can be removed.
        var questions = await _db.Questions
            .IgnoreQueryFilters()
            .Where(q => q.TopicId == id)
            .ToListAsync();

        if (questions.Count == 0) return 0;

        var questionIds = questions.Select(q => q.Id).ToList();

        // Clear Restrict-FK rows that block deleting the questions (same set as DeleteTopicAsync).
        var mockAnswers = await _db.MockExamAnswers
            .Where(a => questionIds.Contains(a.QuestionId))
            .ToListAsync();
        if (mockAnswers.Count > 0)
            _db.MockExamAnswers.RemoveRange(mockAnswers);

        var userAnswers = await _db.UserAnswers
            .Where(a => questionIds.Contains(a.QuestionId))
            .ToListAsync();
        if (userAnswers.Count > 0)
            _db.UserAnswers.RemoveRange(userAnswers);

        await _db.SaveChangesAsync(); // commit FK cleanup before removing questions

        // Remove the questions (AnswerOptions cascade-delete from Question).
        _db.Questions.RemoveRange(questions);
        await _db.SaveChangesAsync();
        _cache.Remove("admin:sections");

        _logger.LogInformation("Cleared {Count} questions from topic {TopicId} (topic kept)",
            questions.Count, id);
        return questions.Count;
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

    public async Task<AdminSectionDto> CreateSectionAsync(CreateSectionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Section name cannot be empty");

        var examType = await _db.ExamTypes.FindAsync(dto.ExamTypeCode)
            ?? throw new ArgumentException($"Exam type '{dto.ExamTypeCode}' not found");

        var exists = await _db.ExamSections.AnyAsync(s => s.Name == dto.Name && s.ExamTypeCode == dto.ExamTypeCode);
        if (exists)
            throw new ArgumentException($"Section '{dto.Name}' already exists for {dto.ExamTypeCode}");

        var section = new ExamSection
        {
            Name = InputSanitizer.Sanitize(dto.Name)!,
            ExamTypeCode = dto.ExamTypeCode
        };

        _db.ExamSections.Add(section);
        await _db.SaveChangesAsync();
        _cache.Remove("admin:sections");

        return new AdminSectionDto(section.Id, section.Name, section.ExamTypeCode);
    }

    public async Task<AdminSectionDto?> UpdateSectionAsync(int id, UpdateSectionDto dto)
    {
        var section = await _db.ExamSections.FindAsync(id);
        if (section == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            var exists = await _db.ExamSections.AnyAsync(s => s.Name == dto.Name && s.ExamTypeCode == section.ExamTypeCode && s.Id != id);
            if (exists)
                throw new ArgumentException($"Section '{dto.Name}' already exists for {section.ExamTypeCode}");
            section.Name = InputSanitizer.Sanitize(dto.Name)!;
        }

        await _db.SaveChangesAsync();
        _cache.Remove("admin:sections");

        return new AdminSectionDto(section.Id, section.Name, section.ExamTypeCode);
    }

    public async Task<bool> DeleteSectionAsync(int id)
    {
        var section = await _db.ExamSections
            .Include(s => s.Topics)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (section == null) return false;

        if (section.Topics?.Any() == true)
        {
            var topicIds = section.Topics.Select(t => t.Id).ToList();

            // Load questionIds — needed to clear Restrict-FK rows before Topics cascade to Questions
            // IgnoreQueryFilters to include soft-deleted questions that still exist in the DB.
            var questionIds = await _db.Questions
                .IgnoreQueryFilters()
                .Where(q => topicIds.Contains(q.TopicId))
                .Select(q => q.Id)
                .ToListAsync();

            if (questionIds.Count > 0)
            {
                // MockExamAnswer.QuestionId is Restrict
                var mockAnswers = await _db.MockExamAnswers
                    .Where(a => questionIds.Contains(a.QuestionId))
                    .ToListAsync();
                if (mockAnswers.Count > 0)
                    _db.MockExamAnswers.RemoveRange(mockAnswers);

                // UserAnswer.AnswerOptionId is Restrict on AnswerOption (AnswerOption cascades from Question)
                var userAnswers = await _db.UserAnswers
                    .Where(a => questionIds.Contains(a.QuestionId))
                    .ToListAsync();
                if (userAnswers.Count > 0)
                    _db.UserAnswers.RemoveRange(userAnswers);
            }

            // StudyPlanEntry.TopicId is Restrict
            var planEntries = await _db.StudyPlanEntries
                .Where(e => topicIds.Contains(e.TopicId))
                .ToListAsync();
            if (planEntries.Count > 0)
                _db.StudyPlanEntries.RemoveRange(planEntries);

            // TopicDependency.PrerequisiteTopicId is Restrict
            var prereqDeps = await _db.TopicDependencies
                .Where(d => topicIds.Contains(d.PrerequisiteTopicId))
                .ToListAsync();
            if (prereqDeps.Count > 0)
                _db.TopicDependencies.RemoveRange(prereqDeps);

            await _db.SaveChangesAsync(); // commit phase 1 before Topics cascade-delete Questions

            _db.Topics.RemoveRange(section.Topics);
        }

        _db.ExamSections.Remove(section);
        await _db.SaveChangesAsync();
        _cache.Remove("admin:sections");

        _logger.LogInformation("Deleted section {SectionId}", id);
        return true;
    }

    // ═══════════════════════════════════════════════════════
    //  EXAM TYPES — CRUD
    // ═══════════════════════════════════════════════════════

    public async Task<List<ExamTypeDto>> GetExamTypesAsync()
    {
        var types = await _db.ExamTypes.OrderBy(e => e.Code).ToListAsync();
        return types.Select(e => new ExamTypeDto(e.Code, e.Name)).ToList();
    }

    public async Task<ExamTypeDto> CreateExamTypeAsync(CreateExamTypeDto dto)
    {
        var code = dto.Code.Trim().ToUpper();
        var entity = new ExamType { Code = code, Name = dto.Name.Trim() };
        _db.ExamTypes.Add(entity);
        await _db.SaveChangesAsync();
        _cache.Remove("exams:all");
        return new ExamTypeDto(entity.Code, entity.Name);
    }

    public async Task<ExamTypeDto?> UpdateExamTypeAsync(string code, UpdateExamTypeDto dto)
    {
        var entity = await _db.ExamTypes.FindAsync(code);
        if (entity == null) return null;
        entity.Name = dto.Name.Trim();
        await _db.SaveChangesAsync();
        _cache.Remove("exams:all");
        _cache.Remove($"exams:sections:{code}");
        return new ExamTypeDto(entity.Code, entity.Name);
    }

    public async Task<bool> DeleteExamTypeAsync(string code)
    {
        var entity = await _db.ExamTypes
            .Include(e => e.Sections)
                .ThenInclude(s => s.Topics)
            .FirstOrDefaultAsync(e => e.Code == code);
        if (entity == null) return false;

        var topicIds = entity.Sections
            .SelectMany(s => s.Topics ?? [])
            .Select(t => t.Id)
            .ToList();

        // ── Phase 1: delete all Restrict-FK dependents and commit ─────────────────
        // Phase 2 deletes Topics → DB cascades Questions; anything with a Restrict FK
        // on QuestionId or TopicId must be explicitly deleted here first.

        if (topicIds.Count > 0)
        {
            // Load questionIds to clear per-question Restrict rows
            // IgnoreQueryFilters to include soft-deleted questions that still exist in the DB.
            var questionIds = await _db.Questions
                .IgnoreQueryFilters()
                .Where(q => topicIds.Contains(q.TopicId))
                .Select(q => q.Id)
                .ToListAsync();

            if (questionIds.Count > 0)
            {
                // MockExamAnswer.QuestionId is Restrict (catches cross-exam answers to these questions)
                var mockAnswers = await _db.MockExamAnswers
                    .Where(a => questionIds.Contains(a.QuestionId))
                    .ToListAsync();
                if (mockAnswers.Count > 0)
                    _db.MockExamAnswers.RemoveRange(mockAnswers);

                // UserAnswer.AnswerOptionId is Restrict on AnswerOption (AnswerOption cascades from Question)
                var userAnswers = await _db.UserAnswers
                    .Where(a => questionIds.Contains(a.QuestionId))
                    .ToListAsync();
                if (userAnswers.Count > 0)
                    _db.UserAnswers.RemoveRange(userAnswers);
            }

            // StudyPlanEntry.TopicId is Restrict
            var planEntries = await _db.StudyPlanEntries
                .Where(e => topicIds.Contains(e.TopicId))
                .ToListAsync();
            if (planEntries.Count > 0)
                _db.StudyPlanEntries.RemoveRange(planEntries);

            // TopicDependency.PrerequisiteTopicId is Restrict
            var prereqDeps = await _db.TopicDependencies
                .Where(d => topicIds.Contains(d.PrerequisiteTopicId))
                .ToListAsync();
            if (prereqDeps.Count > 0)
                _db.TopicDependencies.RemoveRange(prereqDeps);
        }

        // TestSession → ExamType is Restrict
        var testSessions = await _db.TestSessions
            .Where(s => s.ExamTypeCode == code)
            .ToListAsync();
        if (testSessions.Count > 0)
            _db.TestSessions.RemoveRange(testSessions);

        // StudyGoal → ExamType is Restrict (cascades StudyPlan→StudyPlanEntry at DB level)
        var studyGoals = await _db.StudyGoals
            .Where(g => g.ExamTypeCode == code)
            .ToListAsync();
        if (studyGoals.Count > 0)
            _db.StudyGoals.RemoveRange(studyGoals);

        // MockExam → ExamType is Restrict (cascades MockExamAttempt→MockExamAnswer at DB level)
        var mockExams = await _db.MockExams
            .Where(m => m.ExamTypeCode == code)
            .ToListAsync();
        if (mockExams.Count > 0)
            _db.MockExams.RemoveRange(mockExams);

        await _db.SaveChangesAsync(); // commit phase 1 before any Topics are deleted

        // ── Phase 2: delete Topics, Sections, ExamType ───────────────────────────
        // Topics cascade to Questions at DB level; all MockExamAnswers are already gone.
        foreach (var section in entity.Sections)
        {
            if (section.Topics?.Any() == true)
                _db.Topics.RemoveRange(section.Topics);
        }
        _db.ExamSections.RemoveRange(entity.Sections);
        _db.ExamTypes.Remove(entity);
        await _db.SaveChangesAsync();

        _cache.Remove("exams:all");
        _cache.Remove($"exams:sections:{code}");
        _cache.Remove("admin:sections");
        _logger.LogInformation("Deleted exam type {Code}", code);
        return true;
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

    // ═══════════════════════════════════════════════════════
    //  TRASH / RECYCLE BIN
    // ═══════════════════════════════════════════════════════

    private const int PurgeRetentionDays = 30;

    public async Task<TrashSummaryDto> GetTrashAsync()
    {
        var deletedQuestions = await _db.Questions
            .IgnoreQueryFilters()
            .Include(q => q.Topic)
            .Where(q => q.IsDeleted)
            .OrderByDescending(q => q.DeletedAt)
            .ToListAsync();

        var deletedUsers = await _db.Users
            .IgnoreQueryFilters()
            .Where(u => u.IsDeleted)
            .OrderByDescending(u => u.DeletedAt)
            .ToListAsync();

        var now = DateTime.UtcNow;

        var items = new List<TrashItemDto>();

        foreach (var q in deletedQuestions)
        {
            var daysLeft = q.DeletedAt.HasValue
                ? Math.Max(0, PurgeRetentionDays - (int)(now - q.DeletedAt.Value).TotalDays)
                : PurgeRetentionDays;

            items.Add(new TrashItemDto(
                Id: q.Id,
                EntityType: "Question",
                DisplayName: q.Text.Length > 80 ? q.Text[..80] + "…" : q.Text,
                Detail: q.Topic?.Name,
                DeletedAt: q.DeletedAt,
                DeletedBy: q.DeletedBy?.ToString(),
                DaysUntilPurge: daysLeft
            ));
        }

        foreach (var u in deletedUsers)
        {
            var daysLeft = u.DeletedAt.HasValue
                ? Math.Max(0, PurgeRetentionDays - (int)(now - u.DeletedAt.Value).TotalDays)
                : PurgeRetentionDays;

            items.Add(new TrashItemDto(
                Id: u.Id,
                EntityType: "User",
                DisplayName: u.Name,
                Detail: u.Email,
                DeletedAt: u.DeletedAt,
                DeletedBy: u.DeletedBy?.ToString(),
                DaysUntilPurge: daysLeft
            ));
        }

        return new TrashSummaryDto(
            TotalUsers: deletedUsers.Count,
            TotalQuestions: deletedQuestions.Count,
            Items: items
        );
    }

    public async Task<bool> HardDeleteQuestionAsync(int id)
    {
        var question = await _db.Questions
            .IgnoreQueryFilters()
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.Id == id && q.IsDeleted);

        if (question == null) return false;

        // Remove UserAnswers that reference this question's answer options (Restrict FK)
        var optionIds = question.AnswerOptions.Select(a => a.Id).ToList();
        if (optionIds.Count > 0)
        {
            var userAnswers = await _db.UserAnswers.Where(ua => optionIds.Contains(ua.AnswerOptionId)).ToListAsync();
            if (userAnswers.Count > 0) _db.UserAnswers.RemoveRange(userAnswers);
        }

        // Remove MockExamAnswers that reference this question (Restrict FK)
        var mockAnswers = await _db.MockExamAnswers.Where(ma => ma.QuestionId == id).ToListAsync();
        if (mockAnswers.Count > 0) _db.MockExamAnswers.RemoveRange(mockAnswers);

        _db.Questions.Remove(question);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Hard-deleted question {QuestionId}", id);
        return true;
    }

    public async Task<bool> HardDeleteUserAsync(int id)
    {
        var user = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted);

        if (user == null) return false;

        // Prevent hard-deleting the last admin
        if (user.Role == UserRole.Admin)
        {
            var adminCount = await _db.Users.CountAsync(u => u.Role == UserRole.Admin);
            if (adminCount <= 1)
                throw new InvalidOperationException("Cannot delete the last admin user");
        }

        var importJobs = await _db.QuestionImportJobs.Where(j => j.AdminUserId == id).ToListAsync();
        if (importJobs.Count > 0) _db.QuestionImportJobs.RemoveRange(importJobs);

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Hard-deleted user {UserId} ({Email})", id, user.Email);
        return true;
    }

    public async Task<int> EmptyTrashAsync()
    {
        var deletedQuestions = await _db.Questions
            .IgnoreQueryFilters()
            .Where(q => q.IsDeleted)
            .ToListAsync();

        var deletedUsers = await _db.Users
            .IgnoreQueryFilters()
            .Where(u => u.IsDeleted)
            .ToListAsync();

        var count = deletedQuestions.Count + deletedUsers.Count;

        if (deletedQuestions.Count > 0)
        {
            var questionIds = deletedQuestions.Select(q => q.Id).ToList();

            // UserAnswer.AnswerOptionId is Restrict on AnswerOption (cascades from Question)
            var userAnswers = await _db.UserAnswers
                .Where(a => questionIds.Contains(a.QuestionId))
                .ToListAsync();
            if (userAnswers.Count > 0) _db.UserAnswers.RemoveRange(userAnswers);

            // MockExamAnswer.QuestionId is Restrict
            var mockAnswers = await _db.MockExamAnswers
                .Where(a => questionIds.Contains(a.QuestionId))
                .ToListAsync();
            if (mockAnswers.Count > 0) _db.MockExamAnswers.RemoveRange(mockAnswers);

            _db.Questions.RemoveRange(deletedQuestions);
        }

        if (deletedUsers.Count > 0)
        {
            var userIds = deletedUsers.Select(u => u.Id).ToList();

            // QuestionImportJob.AdminUserId is Restrict
            var importJobs = await _db.QuestionImportJobs.Where(j => userIds.Contains(j.AdminUserId)).ToListAsync();
            if (importJobs.Count > 0) _db.QuestionImportJobs.RemoveRange(importJobs);

            // ReferralUsage.ReferredUserId is Restrict
            var referralUsages = await _db.ReferralUsages.Where(r => userIds.Contains(r.ReferredUserId)).ToListAsync();
            if (referralUsages.Count > 0) _db.ReferralUsages.RemoveRange(referralUsages);

            _db.Users.RemoveRange(deletedUsers);
        }

        if (count > 0)
            await _db.SaveChangesAsync();

        _logger.LogInformation("Emptied trash: {Count} records permanently removed", count);
        return count;
    }
}
