using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IDiagnosticService
{
    Task<DiagnosticSessionDto> StartAsync(int userId, string examTypeCode);

    Task<DiagnosticQuestionDto?> GetCurrentQuestionAsync(int userId, int sessionId);

    Task<DiagnosticAnswerResultDto> SubmitAnswerAsync(int userId, DiagnosticAnswerDto dto);

    Task<DiagnosticResultDto> GetResultsAsync(int userId, int sessionId);
}
