namespace UniStart.Application.DTOs;

public record TopicLessonDto(
    int Id,
    int TopicId,
    string TopicName,
    string Title,
    string Content,
    string? VideoUrl,
    int SortOrder
);

public record TopicWithLessonsDto(
    int TopicId,
    string TopicName,
    int LessonCount,
    IEnumerable<TopicLessonSummaryDto> Lessons
);

public record TopicLessonSummaryDto(
    int Id,
    string Title,
    string? VideoUrl,
    int SortOrder
);
