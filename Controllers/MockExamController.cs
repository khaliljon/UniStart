using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/mock-exams")]
[Authorize]
[ApiVersion("1.0")]
[FeatureGate("MockExams")]
public class MockExamController : ControllerBase
{
    private readonly IMockExamService _mockExamService;
    private readonly ISubscriptionService _subscriptionService;

    public MockExamController(IMockExamService mockExamService, ISubscriptionService subscriptionService)
    {
        _mockExamService = mockExamService;
        _subscriptionService = subscriptionService;
    }

    /// <summary>List available mock exams</summary>
    [HttpGet]
    public async Task<IActionResult> GetAvailableMockExams()
    {
        var userId = GetUserId();
        var exams = await _mockExamService.GetAvailableMockExamsAsync(userId);
        return Ok(exams);
    }

    /// <summary>Get mock exam details with sections</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMockExamDetail(int id)
    {
        var exam = await _mockExamService.GetMockExamDetailAsync(id);
        return exam == null ? NotFound() : Ok(exam);
    }

    /// <summary>Start a new mock exam attempt</summary>
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartMockExam(int id, [FromBody] StartMockExamRequest? request = null)
    {
        try
        {
            var userId = GetUserId();
            var attempt = await _mockExamService.StartMockExamAsync(userId, id, request?.SelectedSectionIds);
            return Ok(attempt);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public class StartMockExamRequest
    {
        public List<int>? SelectedSectionIds { get; set; }
    }

    /// <summary>Get current section state (questions + answers)</summary>
    [HttpGet("attempts/{attemptId}/current-section")]
    public async Task<IActionResult> GetCurrentSection(int attemptId)
    {
        var userId = GetUserId();
        var section = await _mockExamService.GetCurrentSectionAsync(userId, attemptId);
        return section == null ? NotFound() : Ok(section);
    }

    /// <summary>Get a specific section by index</summary>
    [HttpGet("attempts/{attemptId}/sections/{sectionIndex}")]
    public async Task<IActionResult> GetSection(int attemptId, int sectionIndex)
    {
        var userId = GetUserId();
        var section = await _mockExamService.GetSectionAsync(userId, attemptId, sectionIndex);
        return section == null ? NotFound() : Ok(section);
    }

    /// <summary>Submit or update an answer</summary>
    [HttpPost("attempts/{attemptId}/answer")]
    public async Task<IActionResult> SubmitAnswer(int attemptId, [FromBody] MockExamSubmitAnswerDto dto)
    {
        var userId = GetUserId();

        // Server-side daily question limit enforcement (Free tier)
        if (!await _subscriptionService.CanAnswerQuestionAsync(userId))
            return StatusCode(429, new { error = "Дневной лимит вопросов исчерпан. Перейдите на Pro для безлимитного доступа." });

        var success = await _mockExamService.SubmitAnswerAsync(userId, attemptId, dto);
        return success ? Ok(new { success = true }) : BadRequest(new { error = "Cannot submit answer" });
    }

    /// <summary>Complete current section and advance to next</summary>
    [HttpPost("attempts/{attemptId}/complete-section")]
    public async Task<IActionResult> CompleteSection(int attemptId)
    {
        var userId = GetUserId();
        var attempt = await _mockExamService.CompleteSectionAsync(userId, attemptId);
        return attempt == null ? BadRequest(new { error = "Cannot complete section" }) : Ok(attempt);
    }

    /// <summary>Complete the entire exam</summary>
    [HttpPost("attempts/{attemptId}/complete-exam")]
    public async Task<IActionResult> CompleteExam(int attemptId)
    {
        var userId = GetUserId();
        var attempt = await _mockExamService.CompleteExamAsync(userId, attemptId);
        return attempt == null ? BadRequest(new { error = "Cannot complete exam" }) : Ok(attempt);
    }

    /// <summary>Get mock exam results (after completion)</summary>
    [HttpGet("attempts/{attemptId}/results")]
    public async Task<IActionResult> GetResults(int attemptId)
    {
        var userId = GetUserId();
        var results = await _mockExamService.GetResultsAsync(userId, attemptId);
        return results == null ? NotFound() : Ok(results);
    }

    /// <summary>Get user's mock exam history</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = GetUserId();
        var history = await _mockExamService.GetHistoryAsync(userId);
        return Ok(history);
    }

    /// <summary>Get user's active (in_progress) attempt, if any</summary>
    [HttpGet("active-attempt")]
    public async Task<IActionResult> GetActiveAttempt()
    {
        var userId = GetUserId();
        var attempt = await _mockExamService.GetActiveAttemptAsync(userId);
        if (attempt == null) return NoContent();
        return Ok(attempt);
    }

    /// <summary>Abandon an in-progress attempt</summary>
    [HttpPost("attempts/{attemptId}/abandon")]
    public async Task<IActionResult> AbandonAttempt(int attemptId)
    {
        var userId = GetUserId();
        var success = await _mockExamService.AbandonAttemptAsync(userId, attemptId);
        return success ? Ok(new { success = true }) : BadRequest(new { error = "No active attempt found" });
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId) || userId <= 0)
            throw new UnauthorizedAccessException("Invalid user identity");
        return userId;
    }
}
