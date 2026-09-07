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

    [HttpGet]
    public async Task<IActionResult> GetAvailableMockExams()
    {
        var userId = GetUserId();
        var exams = await _mockExamService.GetAvailableMockExamsAsync(userId);
        return Ok(exams);
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicMockExams()
    {
        var exams = await _mockExamService.GetAvailableMockExamsAsync(0);
        return Ok(exams);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMockExamDetail(int id)
    {
        var exam = await _mockExamService.GetMockExamDetailAsync(id);
        return exam == null ? NotFound() : Ok(exam);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartMockExam(int id, [FromBody] StartMockExamRequest? request = null)
    {
        try
        {
            var userId = GetUserId();
            var attempt = await _mockExamService.StartMockExamAsync(userId, id, request?.SelectedSectionIds, request?.Language ?? "en");
            return Ok(attempt);
        }
        catch (InvalidOperationException ex) when (ex.Message == "MOCK_LOCKED")
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "MOCK_LOCKED" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public class StartMockExamRequest
    {
        public List<int>? SelectedSectionIds { get; set; }
        public string? Language { get; set; }
    }

    [HttpGet("attempts/{attemptId}/current-section")]
    public async Task<IActionResult> GetCurrentSection(int attemptId)
    {
        var userId = GetUserId();
        var section = await _mockExamService.GetCurrentSectionAsync(userId, attemptId);
        return section == null ? NotFound() : Ok(section);
    }

    [HttpGet("attempts/{attemptId}/sections/{sectionIndex}")]
    public async Task<IActionResult> GetSection(int attemptId, int sectionIndex)
    {
        var userId = GetUserId();
        var section = await _mockExamService.GetSectionAsync(userId, attemptId, sectionIndex);
        return section == null ? NotFound() : Ok(section);
    }

    [HttpPost("attempts/{attemptId}/answer")]
    public async Task<IActionResult> SubmitAnswer(int attemptId, [FromBody] MockExamSubmitAnswerDto dto)
    {
        var userId = GetUserId();

        if (!await _subscriptionService.CanAnswerQuestionAsync(userId))
            return StatusCode(429, new { error = "Дневной лимит вопросов исчерпан. Перейдите на Pro для безлимитного доступа." });

        var success = await _mockExamService.SubmitAnswerAsync(userId, attemptId, dto);
        return success ? Ok(new { success = true }) : BadRequest(new { error = "Cannot submit answer" });
    }

    [HttpPost("attempts/{attemptId}/complete-section")]
    public async Task<IActionResult> CompleteSection(int attemptId)
    {
        var userId = GetUserId();
        var attempt = await _mockExamService.CompleteSectionAsync(userId, attemptId);
        return attempt == null ? BadRequest(new { error = "Cannot complete section" }) : Ok(attempt);
    }

    [HttpPost("attempts/{attemptId}/complete-exam")]
    public async Task<IActionResult> CompleteExam(int attemptId)
    {
        var userId = GetUserId();
        var attempt = await _mockExamService.CompleteExamAsync(userId, attemptId);
        return attempt == null ? BadRequest(new { error = "Cannot complete exam" }) : Ok(attempt);
    }

    [HttpGet("attempts/{attemptId}/results")]
    public async Task<IActionResult> GetResults(int attemptId)
    {
        var userId = GetUserId();
        var results = await _mockExamService.GetResultsAsync(userId, attemptId);
        return results == null ? NotFound() : Ok(results);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = GetUserId();
        var history = await _mockExamService.GetHistoryAsync(userId);
        return Ok(history);
    }

    [HttpGet("active-attempt")]
    public async Task<IActionResult> GetActiveAttempt()
    {
        var userId = GetUserId();
        var attempt = await _mockExamService.GetActiveAttemptAsync(userId);
        if (attempt == null) return NoContent();
        return Ok(attempt);
    }

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
