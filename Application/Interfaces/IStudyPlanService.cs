using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IStudyPlanService
{
    // ─── Goals ───────────────────────────────────────────────
    Task<StudyGoalDto> CreateGoalAsync(int userId, CreateStudyGoalDto dto);
    Task<StudyGoalDto?> GetActiveGoalAsync(int userId);
    Task<StudyGoalDto?> UpdateGoalAsync(int userId, int goalId, UpdateStudyGoalDto dto);
    Task<bool> DeleteGoalAsync(int userId, int goalId);

    // ─── Plan Generation ─────────────────────────────────────
    Task<StudyPlanDto> GeneratePlanAsync(int userId, int goalId);
    Task<StudyPlanDto?> GetActivePlanAsync(int userId);
    Task<StudyPlanDto> RegeneratePlanAsync(int userId);

    // ─── Today's Tasks ───────────────────────────────────────
    Task<TodayPlanDto> GetTodayPlanAsync(int userId);

    // ─── Entry Completion ────────────────────────────────────
    Task<StudyPlanEntryDto?> CompleteEntryAsync(int userId, int entryId, CompleteEntryDto dto);

    // ─── Stats ───────────────────────────────────────────────
    Task<PlanStatsDto> GetPlanStatsAsync(int userId);
}
