using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IAdminService
{
    Task<PagedResult<QuestionListDto>> GetQuestionsAsync(string? examTypeCode = null, string? topicName = null, string? difficulty = null, string? sectionName = null, int page = 1, int pageSize = 50, string? search = null);
    Task<QuestionDetailDto?> GetQuestionByIdAsync(int id);
    Task<QuestionDetailDto> CreateQuestionAsync(CreateQuestionDto dto);
    Task<QuestionDetailDto?> UpdateQuestionAsync(int id, UpdateQuestionDto dto);
    Task<bool> DeleteQuestionAsync(int id);
    Task<BulkImportResultDto> BulkImportAsync(BulkImportDto dto);
    Task<QuestionStatsDto> GetStatsAsync();

    Task<PagedResult<AdminUserDto>> GetUsersAsync(string? role = null, string? search = null, int page = 1, int pageSize = 50, bool includeDeleted = false);
    Task<AdminUserDto?> GetUserByIdAsync(int id);
    Task<AdminUserDto?> UpdateUserAsync(int id, AdminUpdateUserDto dto);
    Task<bool> DeleteUserAsync(int id);
    Task<AdminUserStatsDto> GetUserStatsAsync();

    Task<AdminUserDto?> BlockUserAsync(int id, string? reason = null);
    Task<AdminUserDto?> UnblockUserAsync(int id);
    Task<AdminUserDto?> ResetFreeMockAsync(int id);
    Task<AdminUserDto?> GrantFullAccessAsync(int id);
    Task<AdminUserDto?> RevokeFullAccessAsync(int id);

    Task<AdminDashboardDto> GetDashboardAsync();
    Task<List<AdminTopicSummaryDto>> GetTopicsAsync();

    Task<AdminTopicSummaryDto> CreateTopicAsync(CreateTopicDto dto);
    Task<AdminTopicSummaryDto?> UpdateTopicAsync(int id, UpdateTopicDto dto);
    Task<bool> DeleteTopicAsync(int id);
    Task<int?> ClearTopicQuestionsAsync(int id);
    Task<List<AdminSectionDto>> GetSectionsAsync();

    Task<AdminSectionDto> CreateSectionAsync(CreateSectionDto dto);
    Task<AdminSectionDto?> UpdateSectionAsync(int id, UpdateSectionDto dto);
    Task<bool> DeleteSectionAsync(int id);

    Task<List<ExamTypeDto>> GetExamTypesAsync();
    Task<ExamTypeDto> CreateExamTypeAsync(CreateExamTypeDto dto);
    Task<ExamTypeDto?> UpdateExamTypeAsync(string code, UpdateExamTypeDto dto);
    Task<bool> DeleteExamTypeAsync(string code);

    Task<bool> RestoreQuestionAsync(int id);
    Task<bool> RestoreUserAsync(int id);

    Task<TrashSummaryDto> GetTrashAsync();
    Task<bool> HardDeleteQuestionAsync(int id);
    Task<bool> HardDeleteUserAsync(int id);
    Task<int> EmptyTrashAsync();
}
