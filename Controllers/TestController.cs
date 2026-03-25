using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/test")]
[Authorize]
[ApiVersion("1.0")]
public class TestController : ControllerBase
{
    private readonly IAdaptiveEngineService _adaptiveEngine;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<TestController> _logger;

    public TestController(IAdaptiveEngineService adaptiveEngine, ISubscriptionService subscriptionService, ILogger<TestController> logger)
    {
        _adaptiveEngine = adaptiveEngine;
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    /// <summary>
    /// Get next adaptive question based on user's skill level and exam selection
    /// </summary>
    [HttpPost("next-question")]
    [ProducesResponseType(typeof(NextQuestionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNextQuestion([FromBody] StartTestSessionDto dto)
    {
        var userId = GetCurrentUserId();
        
        var question = await _adaptiveEngine.GetNextQuestionAsync(userId, dto.ExamTypeCodes, dto.SectionId, dto.SectionIds, dto.TopicId);
        var totalQuestions = await _adaptiveEngine.GetTotalQuestionsCountAsync(dto.ExamTypeCodes, dto.SectionId, dto.SectionIds, dto.TopicId);
        var answeredQuestions = await _adaptiveEngine.GetAnsweredQuestionsCountAsync(userId, dto.ExamTypeCodes, dto.SectionId, dto.SectionIds, dto.TopicId);
        var topicMastery = await _adaptiveEngine.GetTopicMasteryAsync(userId, dto.ExamTypeCodes, dto.TopicId);
        
        if (question == null)
        {
            return Ok(new NextQuestionDto(null, true, answeredQuestions, totalQuestions, topicMastery, topicMastery >= 80));
        }

        return Ok(new NextQuestionDto(question, false, answeredQuestions, totalQuestions, topicMastery, topicMastery >= 80));
    }

    /// <summary>
    /// Submit an answer and get result with skill adjustment
    /// </summary>
    [HttpPost("submit-answer")]
    [ProducesResponseType(typeof(AnswerResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerDto dto)
    {
        var userId = GetCurrentUserId();

        // Server-side daily question limit enforcement
        if (!await _subscriptionService.CanAnswerQuestionAsync(userId))
            return StatusCode(429, new { error = "Дневной лимит вопросов исчерпан. Перейдите на Pro для безлимитного доступа." });

        try
        {
            var result = await _adaptiveEngine.ProcessAnswerAsync(userId, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("SubmitAnswer 400 for user {UserId}, question {QuestionId}, session {SessionId}: {Error}",
                userId, dto.QuestionId, dto.TestSessionId, ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get user's current skill profiles
    /// </summary>
    [HttpGet("skill-profiles")]
    [ProducesResponseType(typeof(IEnumerable<UserSkillProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSkillProfiles()
    {
        var userId = GetCurrentUserId();
        var profiles = await _adaptiveEngine.GetUserSkillProfilesAsync(userId);
        return Ok(profiles);
    }

    /// <summary>
    /// Reset user's test progress (clear answers) to allow retaking the test
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetTest()
    {
        var userId = GetCurrentUserId();
        await _adaptiveEngine.ResetUserProgressAsync(userId);
        return Ok(new { message = "Test progress reset successfully" });
    }

    /// <summary>
    /// Get questions answered incorrectly for review/practice
    /// </summary>
    [HttpGet("weak-questions")]
    [ProducesResponseType(typeof(IEnumerable<QuestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeakQuestions([FromQuery] string[]? examTypeCodes = null, [FromQuery] int? count = null)
    {
        var userId = GetCurrentUserId();
        var questions = await _adaptiveEngine.GetIncorrectlyAnsweredQuestionsAsync(userId, examTypeCodes);
        if (count.HasValue && count.Value > 0)
            questions = questions.Take(count.Value).ToList();
        return Ok(questions);
    }

    /// <summary>
    /// Get topics with user progress statistics
    /// </summary>
    [HttpGet("topics")]
    [ProducesResponseType(typeof(IEnumerable<TopicProgressDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopicsWithProgress([FromQuery] string[]? examTypeCodes = null, [FromQuery] int[]? sectionIds = null)
    {
        var userId = GetCurrentUserId();
        var topics = await _adaptiveEngine.GetTopicsWithProgressAsync(userId, examTypeCodes, sectionIds);
        return Ok(topics);
    }

    /// <summary>
    /// Get all questions for a specific topic
    /// </summary>
    [HttpGet("topics/{topicId}/questions")]
    [ProducesResponseType(typeof(IEnumerable<QuestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuestionsByTopic(int topicId)
    {
        var questions = await _adaptiveEngine.GetQuestionsByTopicAsync(topicId);
        return Ok(questions);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                         ?? User.FindFirst("sub");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId) || userId <= 0)
            throw new UnauthorizedAccessException("Invalid user identity");
        return userId;
    }
}
