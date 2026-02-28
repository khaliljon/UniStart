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

    /// <summary>
    /// Get all topics with their lesson summaries
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TopicWithLessonsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTopicLessons([FromQuery] string[]? examTypeCodes = null)
    {
        var result = await _lessonService.GetAllTopicLessonsAsync(examTypeCodes);
        return Ok(result);
    }

    /// <summary>
    /// Get all lessons for a specific topic
    /// </summary>
    [HttpGet("topic/{topicId}")]
    [ProducesResponseType(typeof(IEnumerable<TopicLessonDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLessonsByTopic(int topicId)
    {
        var lessons = await _lessonService.GetLessonsByTopicAsync(topicId);
        return Ok(lessons);
    }

    /// <summary>
    /// Get a single lesson by ID (full content)
    /// </summary>
    [HttpGet("{lessonId}")]
    [ProducesResponseType(typeof(TopicLessonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLesson(int lessonId)
    {
        var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
        if (lesson == null) return NotFound();
        return Ok(lesson);
    }

    /// <summary>
    /// Get hint for a specific question
    /// </summary>
    [HttpGet("hint/{questionId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuestionHint(int questionId)
    {
        var hint = await _lessonService.GetQuestionHintAsync(questionId);
        if (hint == null) return NotFound(new { message = "No hint available for this question" });
        return Ok(new { hint });
    }
}
