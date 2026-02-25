using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IAdminService
{
    Task<List<QuestionListDto>> GetQuestionsAsync(string? examTypeCode = null, string? topicName = null, string? difficulty = null);
    Task<QuestionDetailDto?> GetQuestionByIdAsync(int id);
    Task<QuestionDetailDto> CreateQuestionAsync(CreateQuestionDto dto);
    Task<QuestionDetailDto?> UpdateQuestionAsync(int id, UpdateQuestionDto dto);
    Task<bool> DeleteQuestionAsync(int id);
    Task<BulkImportResultDto> BulkImportAsync(BulkImportDto dto);
    Task<QuestionStatsDto> GetStatsAsync();
}
