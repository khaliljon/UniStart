using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/exams")]
[Authorize]
[ApiVersion("1.0")]
public class ExamsController : ControllerBase
{
    private readonly IExamService _examService;
    private readonly ILogger<ExamsController> _logger;

    public ExamsController(IExamService examService, ILogger<ExamsController> logger)
    {
        _examService = examService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ExamTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllExams()
    {
        var exams = await _examService.GetAllExamsAsync();
        return Ok(exams);
    }

    [HttpGet("{id}/sections")]
    [ProducesResponseType(typeof(IEnumerable<ExamSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExamSections(string id)
    {
        var exam = await _examService.GetExamWithSectionsAsync(id);
        if (exam == null)
        {
            return NotFound(new { error = "Exam not found" });
        }

        return Ok(exam.Sections);
    }
}
