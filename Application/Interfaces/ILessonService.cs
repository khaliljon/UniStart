using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface ILessonService
{
    Task<IEnumerable<TopicWithLessonsDto>> GetAllTopicLessonsAsync(string[]? examTypeCodes = null);
    Task<TopicLessonDto?> GetLessonByIdAsync(int lessonId);
    Task<IEnumerable<TopicLessonDto>> GetLessonsByTopicAsync(int topicId);
    Task<string?> GetQuestionHintAsync(int questionId);

    // Step-based lessons (TH-1)
    Task<LessonWithStepsDto?> GetLessonWithStepsAsync(int userId, int lessonId);
    Task MarkStepCompletedAsync(int userId, int lessonStepId);
    Task<int> GetLessonProgressPercentAsync(int userId, int lessonId);
}
