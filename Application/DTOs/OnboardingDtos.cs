using System.ComponentModel.DataAnnotations;

namespace UniStart.Application.DTOs;


public record CompleteOnboardingDto(
    [Required] string ExamTypeCode,
    [Required] DateTime TargetDate,
    [Required][Range(1, 2400)] int TargetScore
);


public record OnboardingStatusDto(
    bool HasCompletedOnboarding,
    string? ExamTypeCode,
    string? ExamTypeName,
    DateTime? TargetDate,
    int? TargetScore
);

public record ExamTypeInfoDto(
    string Code,
    string Name,
    int MinScore,
    int MaxScore,
    string Description,
    IEnumerable<ExamSectionInfoDto> Sections
);

public record ExamSectionInfoDto(
    int Id,
    string Name,
    int MinScore,
    int MaxScore
);
