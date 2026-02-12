using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;

namespace UniStart.Controllers;

[ApiController]
[Route("api/test")]
[Authorize]
public class TestController : ControllerBase
{
    private readonly IAdaptiveEngineService _adaptiveEngine;
    private readonly ILogger<TestController> _logger;

    public TestController(IAdaptiveEngineService adaptiveEngine, ILogger<TestController> logger)
    {
        _adaptiveEngine = adaptiveEngine;
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
        
        var question = await _adaptiveEngine.GetNextQuestionAsync(userId, dto.ExamTypeCodes, dto.SectionId);
        
        if (question == null)
        {
            return Ok(new NextQuestionDto(null, true, 0, 0));
        }

        return Ok(new NextQuestionDto(question, false, 0, 0));
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

        try
        {
            var result = await _adaptiveEngine.ProcessAnswerAsync(userId, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
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

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                         ?? User.FindFirst("sub");
        return int.Parse(userIdClaim?.Value ?? "0");
    }
}
