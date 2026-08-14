using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IStudyPlanService
{
    Task<StudyGoalDto> CreateGoalAsync(int userId, CreateStudyGoalDto dto);
    Task<StudyGoalDto?> GetActiveGoalAsync(int userId);
    Task<StudyGoalDto?> UpdateGoalAsync(int userId, int goalId, UpdateStudyGoalDto dto);
    Task<bool> DeleteGoalAsync(int userId, int goalId);

    Task<StudyPlanDto> GeneratePlanAsync(int userId, int goalId);
    Task<StudyPlanDto?> GetActivePlanAsync(int userId);
    Task<StudyPlanDto> RegeneratePlanAsync(int userId);

    Task<TodayPlanDto> GetTodayPlanAsync(int userId);

    Task<StudyPlanEntryDto?> CompleteEntryAsync(int userId, int entryId, CompleteEntryDto dto);

    Task<TodayPlanDto> AutoCompleteTodayAsync(int userId);

    Task<PlanStatsDto> GetPlanStatsAsync(int userId);
}
