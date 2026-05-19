using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IAdminService
{
    // Questions
    Task<PagedResult<QuestionListDto>> GetQuestionsAsync(string? examTypeCode = null, string? topicName = null, string? difficulty = null, string? sectionName = null, int page = 1, int pageSize = 50);
    Task<QuestionDetailDto?> GetQuestionByIdAsync(int id);
    Task<QuestionDetailDto> CreateQuestionAsync(CreateQuestionDto dto);
    Task<QuestionDetailDto?> UpdateQuestionAsync(int id, UpdateQuestionDto dto);
    Task<bool> DeleteQuestionAsync(int id);
    Task<BulkImportResultDto> BulkImportAsync(BulkImportDto dto);
    Task<QuestionStatsDto> GetStatsAsync();

    // Users
    Task<PagedResult<AdminUserDto>> GetUsersAsync(string? role = null, string? search = null, int page = 1, int pageSize = 50, bool includeDeleted = false);
    Task<AdminUserDto?> GetUserByIdAsync(int id);
    Task<AdminUserDto?> UpdateUserAsync(int id, AdminUpdateUserDto dto);
    Task<bool> DeleteUserAsync(int id);
    Task<AdminUserStatsDto> GetUserStatsAsync();

    // Block / Suspend (OP-14)
    Task<AdminUserDto?> BlockUserAsync(int id, string? reason = null);
    Task<AdminUserDto?> UnblockUserAsync(int id);

    // Dashboard
    Task<AdminDashboardDto> GetDashboardAsync();
    Task<List<AdminTopicSummaryDto>> GetTopicsAsync();

    // Topics
    Task<AdminTopicSummaryDto> CreateTopicAsync(CreateTopicDto dto);
    Task<AdminTopicSummaryDto?> UpdateTopicAsync(int id, UpdateTopicDto dto);
    Task<bool> DeleteTopicAsync(int id);
    Task<List<AdminSectionDto>> GetSectionsAsync();
    Task<List<AdminSkillDto>> GetSkillsAsync();

    // Sections
    Task<AdminSectionDto> CreateSectionAsync(CreateSectionDto dto);
    Task<AdminSectionDto?> UpdateSectionAsync(int id, UpdateSectionDto dto);
    Task<bool> DeleteSectionAsync(int id);

    // Soft Delete restore (OP-9)
    Task<bool> RestoreQuestionAsync(int id);
    Task<bool> RestoreUserAsync(int id);

    // Trash / Recycle Bin
    Task<TrashSummaryDto> GetTrashAsync();
    Task<bool> HardDeleteQuestionAsync(int id);
    Task<bool> HardDeleteUserAsync(int id);
    Task<int> EmptyTrashAsync();
}
