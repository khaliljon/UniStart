using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/lessons")]
[Authorize]
[ApiVersion("1.0")]
public class LessonController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TopicWithLessonsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTopicLessons([FromQuery] string[]? examTypeCodes = null)
    {
        var result = await _lessonService.GetAllTopicLessonsAsync(examTypeCodes);
        return Ok(result);
    }

    [HttpGet("topic/{topicId}")]
    [ProducesResponseType(typeof(IEnumerable<TopicLessonDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLessonsByTopic(int topicId)
    {
        var lessons = await _lessonService.GetLessonsByTopicAsync(topicId);
        return Ok(lessons);
    }

    [HttpGet("{lessonId}")]
    [ProducesResponseType(typeof(TopicLessonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLesson(int lessonId)
    {
        var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
        if (lesson == null) return NotFound();
        return Ok(lesson);
    }

    [HttpGet("hint/{questionId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionHint(int questionId)
    {
        var hint = await _lessonService.GetQuestionHintAsync(questionId);
        if (hint == null) return NotFound(new { message = "No hint available for this question" });
        return Ok(new { hint });
    }

    [HttpGet("{lessonId}/steps")]
    [ProducesResponseType(typeof(LessonWithStepsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLessonWithSteps(int lessonId)
    {
        var userId = GetCurrentUserId();
        var result = await _lessonService.GetLessonWithStepsAsync(userId, lessonId);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("steps/{stepId}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkStepCompleted(int stepId)
    {
        var userId = GetCurrentUserId();
        await _lessonService.MarkStepCompletedAsync(userId, stepId);
        return NoContent();
    }

    [HttpGet("{lessonId}/progress")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLessonProgress(int lessonId)
    {
        var userId = GetCurrentUserId();
        var percent = await _lessonService.GetLessonProgressPercentAsync(userId, lessonId);
        return Ok(new { lessonId, progressPercent = percent });
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
