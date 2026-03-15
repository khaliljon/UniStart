using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/diagnostic")]
[Authorize]
[ApiVersion("1.0")]
public class DiagnosticController : ControllerBase
{
    private readonly IDiagnosticService _diagnosticService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<DiagnosticController> _logger;

    public DiagnosticController(IDiagnosticService diagnosticService, ISubscriptionService subscriptionService, ILogger<DiagnosticController> logger)
    {
        _diagnosticService = diagnosticService;
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        if (int.TryParse(claim, out var id) && id > 0) return id;
        throw new UnauthorizedAccessException("Invalid user identity");
    }

    /// <summary>
    /// Start a diagnostic test for a specific exam type
    /// </summary>
    [HttpPost("start")]
    [ProducesResponseType(typeof(DiagnosticSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Start([FromBody] StartDiagnosticDto dto)
    {
        try
        {
            var result = await _diagnosticService.StartAsync(GetUserId(), dto.ExamTypeCode);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get current question in the diagnostic test
    /// </summary>
    [HttpGet("{sessionId}/current")]
    [ProducesResponseType(typeof(DiagnosticQuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentQuestion(int sessionId)
    {
        try
        {
            var question = await _diagnosticService.GetCurrentQuestionAsync(GetUserId(), sessionId);
            if (question == null)
                return NotFound(new { error = "No more questions or diagnostic completed" });
            return Ok(question);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Submit an answer for the diagnostic test
    /// </summary>
    [HttpPost("answer")]
    [ProducesResponseType(typeof(DiagnosticAnswerResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SubmitAnswer([FromBody] DiagnosticAnswerDto dto)
    {
        var userId = GetUserId();
        if (!await _subscriptionService.CanAnswerQuestionAsync(userId))
            return StatusCode(429, new { error = "Daily question limit reached. Upgrade to Pro for unlimited access." });

        try
        {
            var result = await _diagnosticService.SubmitAnswerAsync(userId, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get diagnostic test results
    /// </summary>
    [HttpGet("{sessionId}/results")]
    [ProducesResponseType(typeof(DiagnosticResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetResults(int sessionId)
    {
        try
        {
            var result = await _diagnosticService.GetResultsAsync(GetUserId(), sessionId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
