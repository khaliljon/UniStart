using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;

namespace UniStart.Controllers;

[ApiController]
[Route("api/prediction")]
[Authorize]
public class PredictionController : ControllerBase
{
    private readonly IScorePredictionService _service;
    private readonly ILogger<PredictionController> _logger;

    public PredictionController(IScorePredictionService service, ILogger<PredictionController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Прогноз результата по данному экзамену
    /// </summary>
    [HttpGet("{examTypeCode}")]
    public async Task<ActionResult<ScorePredictionDto>> PredictScore(string examTypeCode)
    {
        var userId = GetCurrentUserId();
        try
        {
            var result = await _service.PredictScoreAsync(userId, examTypeCode);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// What-if: что будет, если улучшить тему до заданного уровня?
    /// </summary>
    [HttpPost("what-if")]
    public async Task<ActionResult<WhatIfResultDto>> WhatIf([FromBody] WhatIfRequest request)
    {
        var userId = GetCurrentUserId();
        try
        {
            var result = await _service.WhatIfAsync(userId, request.ExamTypeCode, request.TopicId, request.ImprovedLevel);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// История прогнозов за N дней
    /// </summary>
    [HttpGet("history/{examTypeCode}")]
    public async Task<ActionResult<IEnumerable<PredictionHistoryDto>>> GetHistory(
        string examTypeCode, [FromQuery] int days = 30)
    {
        var userId = GetCurrentUserId();
        var result = await _service.GetPredictionHistoryAsync(userId, examTypeCode, days);
        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub")
            ?? throw new UnauthorizedAccessException("User ID not found in token");
        return int.Parse(claim.Value);
    }
}

/// <summary>
/// Запрос для what-if сценария 
/// </summary>
public record WhatIfRequest(string ExamTypeCode, int TopicId, int ImprovedLevel);
