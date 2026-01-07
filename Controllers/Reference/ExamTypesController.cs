using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.DTOs;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Controllers.Reference
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamTypesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExamTypesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить все типы экзаменов
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExamTypeDto>>> GetExamTypes()
        {
            var examTypes = await _context.ExamTypes
                .Include(et => et.DefaultCountry)
                .Include(et => et.Exams)
                .Where(et => et.IsActive)
                .OrderBy(et => et.Name)
                .Select(et => new ExamTypeDto
                {
                    Id = et.Id,
                    Name = et.Name,
                    NameEn = et.NameEn,
                    Code = et.Code,
                    Description = et.Description,
                    DefaultCountryId = et.DefaultCountryId,
                    DefaultCountryName = et.DefaultCountry != null ? et.DefaultCountry.Name : null,
                    IsActive = et.IsActive,
                    CreatedAt = et.CreatedAt,
                    ExamsCount = et.Exams.Count
                })
                .ToListAsync();

            return Ok(examTypes);
        }

        /// <summary>
        /// Получить тип экзамена по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ExamTypeDto>> GetExamType(int id)
        {
            var examType = await _context.ExamTypes
                .Include(et => et.DefaultCountry)
                .Include(et => et.Exams)
                .Where(et => et.Id == id)
                .Select(et => new ExamTypeDto
                {
                    Id = et.Id,
                    Name = et.Name,
                    NameEn = et.NameEn,
                    Code = et.Code,
                    Description = et.Description,
                    DefaultCountryId = et.DefaultCountryId,
                    DefaultCountryName = et.DefaultCountry != null ? et.DefaultCountry.Name : null,
                    IsActive = et.IsActive,
                    CreatedAt = et.CreatedAt,
                    ExamsCount = et.Exams.Count
                })
                .FirstOrDefaultAsync();

            if (examType == null)
                return NotFound();

            return Ok(examType);
        }

        /// <summary>
        /// Создать тип экзамена (только админ)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExamType>> CreateExamType(CreateExamTypeDto dto)
        {
            if (dto.DefaultCountryId.HasValue)
            {
                var country = await _context.Countries.FindAsync(dto.DefaultCountryId.Value);
                if (country == null)
                    return BadRequest(new { message = "Страна не найдена" });
            }

            var examType = new ExamType
            {
                Name = dto.Name,
                NameEn = dto.NameEn,
                Code = dto.Code.ToUpper(),
                Description = dto.Description,
                DefaultCountryId = dto.DefaultCountryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.ExamTypes.Add(examType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetExamType), new { id = examType.Id }, examType);
        }

        /// <summary>
        /// Обновить тип экзамена (только админ)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateExamType(int id, UpdateExamTypeDto dto)
        {
            var examType = await _context.ExamTypes.FindAsync(id);
            if (examType == null)
                return NotFound();

            if (dto.DefaultCountryId.HasValue)
            {
                var country = await _context.Countries.FindAsync(dto.DefaultCountryId.Value);
                if (country == null)
                    return BadRequest(new { message = "Страна не найдена" });
            }

            examType.Name = dto.Name;
            examType.NameEn = dto.NameEn;
            examType.Code = dto.Code.ToUpper();
            examType.Description = dto.Description;
            examType.DefaultCountryId = dto.DefaultCountryId;
            examType.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Удалить тип экзамена (только админ)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteExamType(int id)
        {
            var examType = await _context.ExamTypes
                .Include(et => et.Exams)
                .FirstOrDefaultAsync(et => et.Id == id);

            if (examType == null)
                return NotFound();

            if (examType.Exams.Any())
                return BadRequest(new { message = "Невозможно удалить тип экзамена, к которому привязаны экзамены" });

            _context.ExamTypes.Remove(examType);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
