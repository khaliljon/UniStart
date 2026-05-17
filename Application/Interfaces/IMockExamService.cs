using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IMockExamService
{
    /// <summary>List all active mock exams with user's attempt stats</summary>
    Task<IEnumerable<MockExamListDto>> GetAvailableMockExamsAsync(int userId);

    /// <summary>Get full mock exam details with sections</summary>
    Task<MockExamDetailDto?> GetMockExamDetailAsync(int mockExamId);

    /// <summary>Start a new mock exam attempt (optionally with selected section IDs for configurable exams)</summary>
    Task<MockExamAttemptDto> StartMockExamAsync(int userId, int mockExamId, List<int>? selectedSectionIds = null);

    /// <summary>Get the current section's questions for an active attempt</summary>
    Task<MockExamSectionStateDto?> GetCurrentSectionAsync(int userId, int attemptId);

    /// <summary>Get a specific section's questions</summary>
    Task<MockExamSectionStateDto?> GetSectionAsync(int userId, int attemptId, int sectionIndex);

    /// <summary>Submit or update an answer within a mock exam</summary>
    Task<bool> SubmitAnswerAsync(int userId, int attemptId, MockExamSubmitAnswerDto dto);

    /// <summary>Complete current section and move to next (or complete exam if last section)</summary>
    Task<MockExamAttemptDto?> CompleteSectionAsync(int userId, int attemptId);

    /// <summary>Complete the entire exam immediately</summary>
    Task<MockExamAttemptDto?> CompleteExamAsync(int userId, int attemptId);

    /// <summary>Get mock exam results after completion</summary>
    Task<MockExamResultDto?> GetResultsAsync(int userId, int attemptId);

    /// <summary>Get user's mock exam attempt history</summary>
    Task<IEnumerable<MockExamHistoryDto>> GetHistoryAsync(int userId);

    /// <summary>Abandon an in-progress attempt</summary>
    Task<bool> AbandonAttemptAsync(int userId, int attemptId);

    /// <summary>Get the user's active (in_progress) attempt, if any</summary>
    Task<MockExamAttemptDto?> GetActiveAttemptAsync(int userId);
}
