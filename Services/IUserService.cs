using UniStart.DTOs;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Services;

/// <summary>
/// Service for User management business logic
/// </summary>
public interface IUserService
{
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(int page, int pageSize);
    Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    Task<bool> AddUserToRoleAsync(string userId, string roleName);
    Task<bool> RemoveUserFromRoleAsync(string userId, string roleName);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto dto);
    Task<Dictionary<string, object>> GetUserStatisticsAsync(string userId);
    Task UpdateLastLoginAsync(string userId);
}
