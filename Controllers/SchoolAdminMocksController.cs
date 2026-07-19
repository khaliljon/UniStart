using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

[ApiController]
[Route("api/school-admin/mocks")]
[Authorize(Roles = "Admin")]
public class SchoolAdminMocksController : ControllerBase
{
    private readonly UniStartDbContext _db;

    public SchoolAdminMocksController(UniStartDbContext db)
    {
        _db = db;
    }

    /// <summary>GET /api/school-admin/mocks/exam-types</summary>
    [HttpGet("exam-types")]
    public async Task<IActionResult> GetExamTypes()
    {
        var types = await _db.ExamTypes
            .AsNoTracking()
            .Select(t => new { code = t.Code, name = t.Name })
            .ToListAsync();
        return Ok(types);
    }

    /// <summary>GET /api/school-admin/mocks/exam-types/{code}/sections</summary>
    [HttpGet("exam-types/{code}/sections")]
    public async Task<IActionResult> GetExamSections(string code)
    {
        var sections = await _db.ExamSections
            .Where(s => s.ExamTypeCode == code)
            .AsNoTracking()
            .Select(s => new
            {
                id = s.Id,
                name = s.Name,
                // How many questions exist in the bank for this section (drives the
                // admin "enough questions?" indicator).
                availableQuestions = _db.Questions.Count(q => q.Topic.SectionId == s.Id),
            })
            .ToListAsync();
        return Ok(sections);
    }

    /// <summary>GET /api/school-admin/mocks</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var exams = await _db.MockExams
            .Include(m => m.Sections)
            .Include(m => m.Attempts)
            .AsNoTracking()
            .ToListAsync();

        var result = exams.Select(m => new AdminMockExamListItemDto(
            m.Id,
            m.ExamTypeCode,
            m.Title,
            m.Description,
            m.TotalTimeMinutes,
            m.IsActive,
            m.Sections.Count,
            m.Sections.Sum(s => s.QuestionCount),
            m.Attempts.Count,
            m.TitleKz,
            m.TitleEn,
            m.DescriptionKz,
            m.DescriptionEn
        ));

        return Ok(result);
    }

    /// <summary>GET /api/school-admin/mocks/{id}</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var exam = await _db.MockExams
            .Include(m => m.Sections)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (exam == null) return NotFound();

        var dto = new AdminMockExamDetailDto(
            exam.Id,
            exam.ExamTypeCode,
            exam.Title,
            exam.Description,
            exam.TotalTimeMinutes,
            exam.IsActive,
            exam.Sections.OrderBy(s => s.SortOrder).Select(s => new AdminMockSectionDto(
                s.Id,
                s.ExamSectionId,
                s.Name,
                s.TimeLimitMinutes,
                s.QuestionCount,
                s.SortOrder,
                s.Instructions
            )),
            exam.TitleKz,
            exam.TitleEn,
            exam.DescriptionKz,
            exam.DescriptionEn
        );

        return Ok(dto);
    }

    /// <summary>POST /api/school-admin/mocks</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveMockExamDto dto)
    {
        var exam = new MockExam
        {
            ExamTypeCode = dto.ExamTypeCode,
            Title = dto.Title,
            Description = dto.Description,
            TitleKz = string.IsNullOrWhiteSpace(dto.TitleKz) ? null : dto.TitleKz.Trim(),
            TitleEn = string.IsNullOrWhiteSpace(dto.TitleEn) ? null : dto.TitleEn.Trim(),
            DescriptionKz = string.IsNullOrWhiteSpace(dto.DescriptionKz) ? null : dto.DescriptionKz.Trim(),
            DescriptionEn = string.IsNullOrWhiteSpace(dto.DescriptionEn) ? null : dto.DescriptionEn.Trim(),
            TotalTimeMinutes = dto.TotalTimeMinutes,
            IsActive = dto.IsActive,
        };

        foreach (var s in dto.Sections)
        {
            exam.Sections.Add(new MockExamSection
            {
                ExamSectionId = s.ExamSectionId,
                Name = s.Name,
                TimeLimitMinutes = s.TimeLimitMinutes,
                QuestionCount = s.QuestionCount,
                SortOrder = s.SortOrder,
                Instructions = s.Instructions
            });
        }

        _db.MockExams.Add(exam);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = exam.Id }, new AdminMockExamDetailDto(
            exam.Id,
            exam.ExamTypeCode,
            exam.Title,
            exam.Description,
            exam.TotalTimeMinutes,
            exam.IsActive,
            exam.Sections.Select(s => new AdminMockSectionDto(
                s.Id,
                s.ExamSectionId,
                s.Name,
                s.TimeLimitMinutes,
                s.QuestionCount,
                s.SortOrder,
                s.Instructions
            )),
            exam.TitleKz,
            exam.TitleEn,
            exam.DescriptionKz,
            exam.DescriptionEn
        ));
    }

    /// <summary>PUT /api/school-admin/mocks/{id}</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveMockExamDto dto)
    {
        var exam = await _db.MockExams
            .Include(m => m.Sections)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (exam == null) return NotFound();

        exam.ExamTypeCode = dto.ExamTypeCode;
        exam.Title = dto.Title;
        exam.Description = dto.Description;
        exam.TitleKz = string.IsNullOrWhiteSpace(dto.TitleKz) ? null : dto.TitleKz.Trim();
        exam.TitleEn = string.IsNullOrWhiteSpace(dto.TitleEn) ? null : dto.TitleEn.Trim();
        exam.DescriptionKz = string.IsNullOrWhiteSpace(dto.DescriptionKz) ? null : dto.DescriptionKz.Trim();
        exam.DescriptionEn = string.IsNullOrWhiteSpace(dto.DescriptionEn) ? null : dto.DescriptionEn.Trim();
        exam.TotalTimeMinutes = dto.TotalTimeMinutes;
        exam.IsActive = dto.IsActive;

        // Simple sync strategy for sections: remove old ones, add new ones
        _db.MockExamSections.RemoveRange(exam.Sections);
        exam.Sections.Clear();

        foreach (var s in dto.Sections)
        {
            exam.Sections.Add(new MockExamSection
            {
                ExamSectionId = s.ExamSectionId,
                Name = s.Name,
                TimeLimitMinutes = s.TimeLimitMinutes,
                QuestionCount = s.QuestionCount,
                SortOrder = s.SortOrder,
                Instructions = s.Instructions
            });
        }

        await _db.SaveChangesAsync();

        return Ok(new AdminMockExamDetailDto(
            exam.Id,
            exam.ExamTypeCode,
            exam.Title,
            exam.Description,
            exam.TotalTimeMinutes,
            exam.IsActive,
            exam.Sections.Select(s => new AdminMockSectionDto(
                s.Id,
                s.ExamSectionId,
                s.Name,
                s.TimeLimitMinutes,
                s.QuestionCount,
                s.SortOrder,
                s.Instructions
            )),
            exam.TitleKz,
            exam.TitleEn,
            exam.DescriptionKz,
            exam.DescriptionEn
        ));
    }

    /// <summary>PUT /api/school-admin/mocks/{id}/active</summary>
    [HttpPut("{id:int}/active")]
    public async Task<IActionResult> ToggleActive(int id, [FromBody] ToggleActiveDto dto)
    {
        var exam = await _db.MockExams.FindAsync(id);
        if (exam == null) return NotFound();

        exam.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();

        return Ok(new { id = exam.Id, isActive = exam.IsActive });
    }

    /// <summary>DELETE /api/school-admin/mocks/{id}</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var exam = await _db.MockExams.FindAsync(id);
        if (exam == null) return NotFound();

        _db.MockExams.Remove(exam);
        await _db.SaveChangesAsync();

        return Ok(new { deleted = true });
    }

    public record ToggleActiveDto(bool IsActive);
}
