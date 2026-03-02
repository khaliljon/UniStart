using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IMistakeService
{
    Task<IEnumerable<MistakeEntryDto>> GetMistakesAsync(int userId, string? examTypeCode = null, int? topicId = null, string? errorType = null, int page = 1, int pageSize = 20);
    Task SetErrorTypeAsync(int userId, SetErrorTypeRequest request);
    Task SetNoteAsync(int userId, SetMistakeNoteRequest request);
    Task<MistakeAnalysisDto> GetAnalysisAsync(int userId, string? examTypeCode = null);
    Task<int> GetMistakeCountAsync(int userId);
}
