using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
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
}
