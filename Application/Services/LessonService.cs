using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class LessonService : ILessonService
{
    private readonly UniStartDbContext _context;

    public LessonService(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TopicWithLessonsDto>> GetAllTopicLessonsAsync(string[]? examTypeCodes = null)
    {
        var query = _context.Topics
            .Include(t => t.Lessons)
            .Include(t => t.Section)
            .AsQueryable();

        if (examTypeCodes != null && examTypeCodes.Length > 0)
        {
            query = query.Where(t => t.Section != null && examTypeCodes.Contains(t.Section.ExamTypeCode));
        }

        var topics = await query
            .OrderBy(t => t.Section != null ? t.Section.ExamTypeCode : "")
            .ThenBy(t => t.Name)
            .ToListAsync();

        return topics.Select(t => new TopicWithLessonsDto(
            t.Id,
            t.Name,
            t.Lessons.Count,
            t.Lessons.OrderBy(l => l.SortOrder).Select(l => new TopicLessonSummaryDto(
                l.Id,
                l.Title,
                l.VideoUrl,
                l.SortOrder
            ))
        ));
    }

    public async Task<TopicLessonDto?> GetLessonByIdAsync(int lessonId)
    {
        var lesson = await _context.TopicLessons
            .Include(l => l.Topic)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null) return null;

        return new TopicLessonDto(
            lesson.Id,
            lesson.TopicId,
            lesson.Topic.Name,
            lesson.Title,
            lesson.Content,
            lesson.VideoUrl,
            lesson.SortOrder
        );
    }

    public async Task<IEnumerable<TopicLessonDto>> GetLessonsByTopicAsync(int topicId)
    {
        var lessons = await _context.TopicLessons
            .Include(l => l.Topic)
            .Where(l => l.TopicId == topicId)
            .OrderBy(l => l.SortOrder)
            .ToListAsync();

        return lessons.Select(l => new TopicLessonDto(
            l.Id,
            l.TopicId,
            l.Topic.Name,
            l.Title,
            l.Content,
            l.VideoUrl,
            l.SortOrder
        ));
    }

    public async Task<string?> GetQuestionHintAsync(int questionId)
    {
        var question = await _context.Questions.FindAsync(questionId);
        return question?.Hint;
    }

    // ─── Step-based lessons (TH-1) ─────────────────────────

    public async Task<LessonWithStepsDto?> GetLessonWithStepsAsync(int userId, int lessonId)
    {
        var lesson = await _context.TopicLessons
            .Include(l => l.Topic)
            .Include(l => l.Steps)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null) return null;

        var stepIds = lesson.Steps.Select(s => s.Id).ToList();
        var completedStepIds = await _context.UserLessonProgress
            .Where(p => p.UserId == userId && stepIds.Contains(p.LessonStepId))
            .Select(p => p.LessonStepId)
            .ToListAsync();

        var steps = lesson.Steps.OrderBy(s => s.SortOrder).Select(s => new LessonStepDto(
            s.Id, s.LessonId, s.Title, s.Content,
            s.StepType.ToString(), s.QuizQuestionId, s.SortOrder
        ));

        return new LessonWithStepsDto(
            lesson.Id, lesson.TopicId, lesson.Topic.Name,
            lesson.Title, lesson.VideoUrl,
            steps, completedStepIds.Count, lesson.Steps.Count
        );
    }

    public async Task MarkStepCompletedAsync(int userId, int lessonStepId)
    {
        var exists = await _context.UserLessonProgress
            .AnyAsync(p => p.UserId == userId && p.LessonStepId == lessonStepId);

        if (!exists)
        {
            _context.UserLessonProgress.Add(new UserLessonProgress
            {
                UserId = userId,
                LessonStepId = lessonStepId,
                CompletedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetLessonProgressPercentAsync(int userId, int lessonId)
    {
        var lesson = await _context.TopicLessons
            .Include(l => l.Steps)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null || lesson.Steps.Count == 0) return 0;

        var stepIds = lesson.Steps.Select(s => s.Id).ToList();
        var completed = await _context.UserLessonProgress
            .CountAsync(p => p.UserId == userId && stepIds.Contains(p.LessonStepId));

        return (int)Math.Round(100.0 * completed / lesson.Steps.Count);
    }
}
