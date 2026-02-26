using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IAdminService
{
    // Questions
    Task<List<QuestionListDto>> GetQuestionsAsync(string? examTypeCode = null, string? topicName = null, string? difficulty = null);
    Task<QuestionDetailDto?> GetQuestionByIdAsync(int id);
    Task<QuestionDetailDto> CreateQuestionAsync(CreateQuestionDto dto);
    Task<QuestionDetailDto?> UpdateQuestionAsync(int id, UpdateQuestionDto dto);
    Task<bool> DeleteQuestionAsync(int id);
    Task<BulkImportResultDto> BulkImportAsync(BulkImportDto dto);
    Task<QuestionStatsDto> GetStatsAsync();

    // Users
    Task<List<AdminUserDto>> GetUsersAsync(string? role = null, string? search = null);
    Task<AdminUserDto?> GetUserByIdAsync(int id);
    Task<AdminUserDto?> UpdateUserAsync(int id, AdminUpdateUserDto dto);
    Task<bool> DeleteUserAsync(int id);
    Task<AdminUserStatsDto> GetUserStatsAsync();

    // Dashboard
    Task<AdminDashboardDto> GetDashboardAsync();
    Task<List<AdminTopicSummaryDto>> GetTopicsAsync();
}
