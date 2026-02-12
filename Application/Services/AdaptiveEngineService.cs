using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Domain.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

/// <summary>
/// Adaptive Engine Service implementing the rules from RULES.md:
/// - Skill Adjustment: +X for correct, -Y for incorrect
/// - Question Selection based on skill level thresholds
/// - Exam filtering by selected ExamType
/// </summary>
public class AdaptiveEngineService : IAdaptiveEngineService
{
    private readonly UniStartDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    // Adaptive rule constants (from RULES.md)
    private const int CorrectAnswerBonus = 5;    // +X for correct answer
    private const int IncorrectAnswerPenalty = 3; // -Y for incorrect answer
    private const int EasyThreshold = 40;         // skill < 40 => easy
    private const int MediumThreshold = 70;       // skill >= 40 && < 70 => medium
    private const int MinSkillLevel = 0;
    private const int MaxSkillLevel = 100;

    public AdaptiveEngineService(UniStartDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Determines difficulty based on skill level per RULES.md:
    /// - skill < 40 => Easy
    /// - skill >= 40 && < 70 => Medium  
    /// - skill >= 70 => Hard
    /// </summary>
    public QuestionDifficulty GetDifficultyForSkillLevel(int skillLevel)
    {
        if (skillLevel < EasyThreshold)
            return QuestionDifficulty.Easy;
        if (skillLevel < MediumThreshold)
            return QuestionDifficulty.Medium;
        return QuestionDifficulty.Hard;
    }

    /// <summary>
    /// Selects next question based on user's skill profile and exam selection
    /// </summary>
    public async Task<QuestionDto?> GetNextQuestionAsync(int userId, string[] examTypeCodes, int? sectionId = null)
    {
        // Get user's answered questions to avoid repetition
        var answeredQuestionIds = await _context.UserAnswers
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.QuestionId)
            .ToListAsync();

        // Build query for available questions
        var questionsQuery = _context.Questions
            .Include(q => q.Topic)
                .ThenInclude(t => t.Section)
            .Include(q => q.AnswerOptions)
            .Where(q => !answeredQuestionIds.Contains(q.Id));

        // Filter by exam type if specified
        if (examTypeCodes.Length > 0)
        {
            questionsQuery = questionsQuery.Where(q => 
                q.Topic.Section != null && 
                examTypeCodes.Contains(q.Topic.Section.ExamTypeCode));
        }

        // Filter by section if specified
        if (sectionId.HasValue)
        {
            questionsQuery = questionsQuery.Where(q => q.Topic.SectionId == sectionId.Value);
        }

        // Get user's average skill level across relevant skills
        var avgSkillLevel = await GetAverageSkillLevelAsync(userId);
        var targetDifficulty = GetDifficultyForSkillLevel(avgSkillLevel);

        // Try to find questions matching the target difficulty
        var question = await questionsQuery
            .Where(q => q.Difficulty == targetDifficulty)
            .OrderBy(q => Guid.NewGuid()) // Random selection
            .FirstOrDefaultAsync();

        // If no questions at target difficulty, try any available question
        if (question == null)
        {
            question = await questionsQuery
                .OrderBy(q => Guid.NewGuid())
                .FirstOrDefaultAsync();
        }

        if (question == null) return null;

        return MapToQuestionDto(question);
    }

    /// <summary>
    /// Processes answer and updates skill per RULES.md:
    /// - Correct => +X skill
    /// - Incorrect => -Y skill
    /// </summary>
    public async Task<AnswerResultDto> ProcessAnswerAsync(int userId, SubmitAnswerDto answer)
    {
        var question = await _context.Questions
            .Include(q => q.AnswerOptions)
            .Include(q => q.Topic)
            .FirstOrDefaultAsync(q => q.Id == answer.QuestionId);

        if (question == null)
            throw new ArgumentException("Question not found");

        var selectedOption = question.AnswerOptions.FirstOrDefault(o => o.Id == answer.AnswerOptionId);
        if (selectedOption == null)
            throw new ArgumentException("Answer option not found");

        var correctOption = question.AnswerOptions.First(o => o.IsCorrect);
        var isCorrect = selectedOption.IsCorrect;

        // Record the answer
        var userAnswer = new UserAnswer
        {
            UserId = userId,
            QuestionId = question.Id,
            AnswerOptionId = answer.AnswerOptionId,
            AnsweredAt = DateTime.UtcNow
        };
        _context.UserAnswers.Add(userAnswer);

        // Update skill level
        var (newLevel, change) = await UpdateSkillLevelAsync(userId, question.Topic.SkillId, isCorrect);

        await _unitOfWork.SaveChangesAsync();

        return new AnswerResultDto(isCorrect, correctOption.Id, newLevel, change);
    }

    /// <summary>
    /// Updates skill level per RULES.md adaptive rules
    /// </summary>
    public async Task<(int newLevel, int change)> UpdateSkillLevelAsync(int userId, int skillId, bool isCorrect)
    {
        var profile = await _context.UserSkillProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId && p.SkillId == skillId);

        if (profile == null)
        {
            // Create new profile if doesn't exist
            profile = new UserSkillProfile
            {
                UserId = userId,
                SkillId = skillId,
                Level = 50 // Default starting level
            };
            _context.UserSkillProfiles.Add(profile);
        }

        var oldLevel = profile.Level;
        var change = isCorrect ? CorrectAnswerBonus : -IncorrectAnswerPenalty;
        
        // Apply change with bounds
        profile.Level = Math.Clamp(profile.Level + change, MinSkillLevel, MaxSkillLevel);
        profile.LastUpdated = DateTime.UtcNow;

        return (profile.Level, profile.Level - oldLevel);
    }

    public async Task<UserSkillProfileDto?> GetUserSkillProfileAsync(int userId, int skillId)
    {
        var profile = await _context.UserSkillProfiles
            .Include(p => p.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.SkillId == skillId);

        if (profile == null) return null;

        return new UserSkillProfileDto(
            profile.SkillId,
            profile.Skill.Name,
            profile.Skill.Code,
            profile.Level,
            profile.LastUpdated
        );
    }

    public async Task<IEnumerable<UserSkillProfileDto>> GetUserSkillProfilesAsync(int userId)
    {
        var profiles = await _context.UserSkillProfiles
            .Include(p => p.Skill)
            .Where(p => p.UserId == userId)
            .ToListAsync();

        return profiles.Select(p => new UserSkillProfileDto(
            p.SkillId,
            p.Skill.Name,
            p.Skill.Code,
            p.Level,
            p.LastUpdated
        ));
    }

    private async Task<int> GetAverageSkillLevelAsync(int userId)
    {
        var avgLevel = await _context.UserSkillProfiles
            .Where(p => p.UserId == userId)
            .AverageAsync(p => (int?)p.Level);

        return (int)(avgLevel ?? 50);
    }

    private static QuestionDto MapToQuestionDto(Question question)
    {
        return new QuestionDto(
            question.Id,
            question.Text,
            question.Difficulty.ToString(),
            question.TopicId,
            question.Topic.Name,
            question.AnswerOptions.Select(o => new AnswerOptionDto(o.Id, o.Text))
        );
    }
}
