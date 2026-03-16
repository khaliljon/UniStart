using UniStart.Application.DTOs;
using UniStart.Domain.Entities;

namespace UniStart.Application.Interfaces;

public interface IAdaptiveEngineService
{
    /// <summary>
    /// Gets the appropriate difficulty level based on user's skill level
    /// </summary>
    QuestionDifficulty GetDifficultyForSkillLevel(int skillLevel);

    /// <summary>
    /// Selects the next question for the user based on their skill profile and exam selection
    /// </summary>
    Task<QuestionDto?> GetNextQuestionAsync(int userId, string[] examTypeCodes, int? sectionId = null, int[]? sectionIds = null, int? topicId = null);

    /// <summary>
    /// Processes a user's answer and updates their skill profile
    /// </summary>
    Task<AnswerResultDto> ProcessAnswerAsync(int userId, SubmitAnswerDto answer);

    /// <summary>
    /// Updates user's skill level based on answer correctness using IRT EAP estimation
    /// </summary>
    Task<(int newLevel, int change, double theta, double thetaSE)> UpdateSkillLevelAsync(int userId, int skillId, bool isCorrect);

    /// <summary>
    /// Gets user's current skill profile for a specific skill
    /// </summary>
    Task<UserSkillProfileDto?> GetUserSkillProfileAsync(int userId, int skillId);

    /// <summary>
    /// Gets all skill profiles for a user
    /// </summary>
    Task<IEnumerable<UserSkillProfileDto>> GetUserSkillProfilesAsync(int userId);

    /// <summary>
    /// Resets user's test progress (clears answers and skill profiles)
    /// </summary>
    Task ResetUserProgressAsync(int userId);

    /// <summary>
    /// Gets total question count for selected exams, optionally filtered by section/topic
    /// </summary>
    Task<int> GetTotalQuestionsCountAsync(string[] examTypeCodes, int? sectionId = null, int[]? sectionIds = null, int? topicId = null);

    /// <summary>
    /// Gets count of answered questions for user in selected exams, optionally filtered by section/topic
    /// </summary>
    Task<int> GetAnsweredQuestionsCountAsync(int userId, string[] examTypeCodes, int? sectionId = null, int[]? sectionIds = null, int? topicId = null);

    /// <summary>
    /// Calculates mastery percentage for a topic (0-100) based on last answer per question
    /// </summary>
    Task<int> GetTopicMasteryAsync(int userId, string[] examTypeCodes, int? topicId = null);

    /// <summary>
    /// Gets questions that user answered incorrectly (for review mode)
    /// </summary>
    Task<IEnumerable<QuestionDto>> GetIncorrectlyAnsweredQuestionsAsync(int userId, string[]? examTypeCodes = null);

    /// <summary>
    /// Gets all topics with progress for user
    /// </summary>
    Task<IEnumerable<TopicProgressDto>> GetTopicsWithProgressAsync(int userId, string[]? examTypeCodes = null);

    /// <summary>
    /// Gets questions by topic
    /// </summary>
    Task<IEnumerable<QuestionDto>> GetQuestionsByTopicAsync(int topicId);
}
