namespace UniStart.Application.DTOs;

public record ExamTypeDto(
    string Code,
    string Name
);

public record CreateExamTypeDto(
    string Code,
    string Name
);

public record UpdateExamTypeDto(
    string Name
);

public record ExamSectionDto(
    int Id,
    string ExamTypeCode,
    string Name,
    int MinScore,
    int MaxScore
);

public record ExamWithSectionsDto(
    string Code,
    string Name,
    IEnumerable<ExamSectionDto> Sections
);
