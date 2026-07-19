namespace UniStart.Application.DTOs;

public record ExamSittingDto(int Id, DateOnly Date, bool IsActive, int SortOrder);

public record SaveExamSittingDto(DateOnly Date, bool IsActive, int SortOrder);
