using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IMockExamService
{
    Task<IEnumerable<MockExamListDto>> GetAvailableMockExamsAsync(int userId);

    Task<MockExamDetailDto?> GetMockExamDetailAsync(int mockExamId);

    Task<MockExamAttemptDto> StartMockExamAsync(int userId, int mockExamId, List<int>? selectedSectionIds = null, string language = "en");

    Task<MockExamSectionStateDto?> GetCurrentSectionAsync(int userId, int attemptId);

    Task<MockExamSectionStateDto?> GetSectionAsync(int userId, int attemptId, int sectionIndex);

    Task<bool> SubmitAnswerAsync(int userId, int attemptId, MockExamSubmitAnswerDto dto);

    Task<MockExamAttemptDto?> CompleteSectionAsync(int userId, int attemptId);

    Task<MockExamAttemptDto?> CompleteExamAsync(int userId, int attemptId);

    Task<MockExamResultDto?> GetResultsAsync(int userId, int attemptId);
    Task<MockExamResultDto?> GetResultsForAdminAsync(int attemptId);

    Task<IEnumerable<MockExamHistoryDto>> GetHistoryAsync(int userId);

    Task<bool> AbandonAttemptAsync(int userId, int attemptId);

    Task<MockExamAttemptDto?> GetActiveAttemptAsync(int userId);

    /// <summary>Idempotently finalizes an in-progress attempt (scores saved answers, sets reason).</summary>
    Task FinalizeAttemptAsync(UniStart.Domain.Entities.MockExamAttempt attempt, string completionReason, DateTime completedAt);

    /// <summary>Hangfire: backfills deadlines and closes orphaned expired attempts. Returns count closed.</summary>
    Task<int> ExpireOrphanedAttemptsAsync();
}
