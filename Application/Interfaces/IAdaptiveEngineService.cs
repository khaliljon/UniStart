using UniStart.Application.DTOs;
using UniStart.Domain.Entities;

namespace UniStart.Application.Interfaces;

public interface IAdaptiveEngineService
{
    QuestionDifficulty GetDifficultyForSkillLevel(int skillLevel);

    Task<QuestionDto?> GetNextQuestionAsync(int userId, string[] examTypeCodes, int? sectionId = null, int[]? sectionIds = null, int? topicId = null);

    Task<AnswerResultDto> ProcessAnswerAsync(int userId, SubmitAnswerDto answer);

    Task<(int newLevel, int change, double theta, double thetaSE)> UpdateSkillLevelAsync(int userId, int skillId, bool isCorrect);

    Task<UserSkillProfileDto?> GetUserSkillProfileAsync(int userId, int skillId);

    Task<IEnumerable<UserSkillProfileDto>> GetUserSkillProfilesAsync(int userId);

    Task ResetUserProgressAsync(int userId);

    Task<int> GetTotalQuestionsCountAsync(string[] examTypeCodes, int? sectionId = null, int[]? sectionIds = null, int? topicId = null);

    Task<int> GetAnsweredQuestionsCountAsync(int userId, string[] examTypeCodes, int? sectionId = null, int[]? sectionIds = null, int? topicId = null);

    Task<int> GetTopicMasteryAsync(int userId, string[] examTypeCodes, int? topicId = null);

    Task<IEnumerable<QuestionDto>> GetIncorrectlyAnsweredQuestionsAsync(int userId, string[]? examTypeCodes = null);

    Task<IEnumerable<TopicProgressDto>> GetTopicsWithProgressAsync(int userId, string[]? examTypeCodes = null, int[]? sectionIds = null);

    Task<IEnumerable<QuestionDto>> GetQuestionsByTopicAsync(int topicId);
}
