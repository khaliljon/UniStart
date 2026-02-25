using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IDiagnosticService
{
    /// <summary>
    /// Start a diagnostic test — selects 10 calibration questions across sections
    /// </summary>
    Task<DiagnosticSessionDto> StartAsync(int userId, string examTypeCode);

    /// <summary>
    /// Get the current (next unanswered) question in the diagnostic
    /// </summary>
    Task<DiagnosticQuestionDto?> GetCurrentQuestionAsync(int userId, int sessionId);

    /// <summary>
    /// Submit an answer for the diagnostic test
    /// </summary>
    Task<DiagnosticAnswerResultDto> SubmitAnswerAsync(int userId, DiagnosticAnswerDto dto);

    /// <summary>
    /// Get results after diagnostic is completed
    /// </summary>
    Task<DiagnosticResultDto> GetResultsAsync(int userId, int sessionId);
}
