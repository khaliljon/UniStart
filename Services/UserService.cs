using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniStart.DTOs;
using UniStart.Models;
using UniStart.Repositories;

namespace UniStart.Services;

/// <summary>
/// Service implementation for User management
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(int page, int pageSize)
    {
        return await _userManager.Users
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Enumerable.Empty<string>();

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _userManager.AddToRoleAsync(user, roleName);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} added to role {Role}", user.Email, roleName);
        }
        
        return result.Succeeded;
    }

    public async Task<bool> RemoveUserFromRoleAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} removed from role {Role}", user.Email, roleName);
        }
        
        return result.Succeeded;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        return await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> UpdateUserProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        if (!string.IsNullOrWhiteSpace(dto.FirstName))
            user.FirstName = dto.FirstName;
        
        if (!string.IsNullOrWhiteSpace(dto.LastName))
            user.LastName = dto.LastName;
        
        if (!string.IsNullOrWhiteSpace(dto.UserName))
            user.UserName = dto.UserName;

        var result = await _userManager.UpdateAsync(user);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("User profile updated: {Email}", user.Email);
        }
        
        return result.Succeeded;
    }

    public async Task<Dictionary<string, object>> GetUserStatisticsAsync(string userId)
    {
        // Quiz statistics
        var quizAttempts = await _unitOfWork.QuizAttempts.GetUserAttemptsAsync(userId);
        var quizStats = new
        {
            TotalAttempts = quizAttempts.Count(),
            UniqueQuizzes = quizAttempts.Select(a => a.QuizId).Distinct().Count(),
            AverageScore = quizAttempts.Any() ? quizAttempts.Average(a => a.Percentage) : 0,
            BestScore = quizAttempts.Any() ? quizAttempts.Max(a => a.Percentage) : 0
        };

        // Exam statistics
        var examAttempts = await _unitOfWork.ExamAttempts.GetUserAttemptsAsync(userId);
        var examStats = new
        {
            TotalAttempts = examAttempts.Count(),
            UniqueExams = examAttempts.Select(a => a.ExamId).Distinct().Count(),
            AverageScore = examAttempts.Any() ? examAttempts.Average(a => a.Percentage) : 0,
            PassedExams = examAttempts.Count(a => a.Passed)
        };

        // Flashcard statistics
        var masteredCards = await _unitOfWork.FlashcardProgress.CountAsync(p => p.UserId == userId && p.IsMastered);
        var reviewedCards = await _unitOfWork.FlashcardProgress.CountAsync(p => p.UserId == userId && p.LastReviewedAt != null);
        var completedSets = await _unitOfWork.Repository<UserFlashcardSetAccess>()
            .CountAsync(a => a.UserId == userId && a.IsCompleted);

        return new Dictionary<string, object>
        {
            ["quiz"] = quizStats,
            ["exam"] = examStats,
            ["flashcards"] = new
            {
                MasteredCards = masteredCards,
                ReviewedCards = reviewedCards,
                CompletedSets = completedSets
            }
        };
    }

    public async Task UpdateLastLoginAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
    }
}
