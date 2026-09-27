namespace UniStart.Application.DTOs;

public record ExamSittingDto(int Id, DateOnly Date, DateOnly? EndDate, bool IsActive, int SortOrder);

public record SaveExamSittingDto(DateOnly Date, DateOnly? EndDate, bool IsActive, int SortOrder);
