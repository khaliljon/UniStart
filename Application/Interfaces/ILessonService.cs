using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface ILessonService
{
    Task<IEnumerable<TopicWithLessonsDto>> GetAllTopicLessonsAsync(string[]? examTypeCodes = null);
    Task<TopicLessonDto?> GetLessonByIdAsync(int lessonId);
    Task<IEnumerable<TopicLessonDto>> GetLessonsByTopicAsync(int topicId);
    Task<string?> GetQuestionHintAsync(int questionId);
}
