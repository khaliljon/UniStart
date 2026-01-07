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
    public class UniversitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UniversitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить все университеты
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UniversityDto>>> GetUniversities([FromQuery] int? countryId = null)
        {
            var query = _context.Universities
                .Include(u => u.Country)
                .Include(u => u.Exams)
                .Include(u => u.ExamTypes)
                .Where(u => u.IsActive);

            if (countryId.HasValue)
                query = query.Where(u => u.CountryId == countryId.Value);

            var universities = await query
                .OrderBy(u => u.Name)
                .Select(u => new UniversityDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    NameEn = u.NameEn,
                    City = u.City,
                    Description = u.Description,
                    Website = u.Website,
                    Type = u.Type.ToString(),
                    IsActive = u.IsActive,
                    CountryId = u.CountryId,
                    CountryName = u.Country.Name,
                    CountryCode = u.Country.Code,
                    ExamTypeIds = u.ExamTypes.Select(et => et.Id).ToList(),
                    CreatedAt = u.CreatedAt,
                    ExamsCount = u.Exams.Count
                })
                .ToListAsync();

            return Ok(universities);
        }

        /// <summary>
        /// Получить университет по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UniversityDto>> GetUniversity(int id)
        {
            var university = await _context.Universities
                .Include(u => u.Country)
                .Include(u => u.Exams)
                .Include(u => u.ExamTypes)
                .Where(u => u.Id == id)
                .Select(u => new UniversityDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    NameEn = u.NameEn,
                    City = u.City,
                    Description = u.Description,
                    Website = u.Website,
                    Type = u.Type.ToString(),
                    IsActive = u.IsActive,
                    CountryId = u.CountryId,
                    CountryName = u.Country.Name,
                    CountryCode = u.Country.Code,
                    ExamTypeIds = u.ExamTypes.Select(et => et.Id).ToList(),
                    CreatedAt = u.CreatedAt,
                    ExamsCount = u.Exams.Count
                })
                .FirstOrDefaultAsync();

            if (university == null)
                return NotFound();

            return Ok(university);
        }

        /// <summary>
        /// Создать университет (только админ)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUniversity([FromBody] CreateUniversityDto dto)
        {
            var country = await _context.Countries.FindAsync(dto.CountryId);
            if (country == null)
                return BadRequest(new { message = "Страна не найдена" });

            var university = new University
            {
                Name = dto.Name,
                NameEn = dto.NameEn,
                City = dto.City,
                Description = dto.Description,
                Website = dto.Website,
                Type = (UniversityType)dto.Type,
                CountryId = dto.CountryId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Добавляем типы экзаменов
            if (dto.ExamTypeIds != null && dto.ExamTypeIds.Any())
            {
                var examTypes = await _context.ExamTypes
                    .Where(et => dto.ExamTypeIds.Contains(et.Id))
                    .ToListAsync();
                university.ExamTypes = examTypes;
            }

            _context.Universities.Add(university);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUniversity), new { id = university.Id }, university);
        }

        /// <summary>
        /// Обновить университет (только админ)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUniversity(int id, [FromBody] UpdateUniversityDto dto)
        {
            var university = await _context.Universities
                .Include(u => u.ExamTypes)
                .FirstOrDefaultAsync(u => u.Id == id);
            
            if (university == null)
                return NotFound();

            var country = await _context.Countries.FindAsync(dto.CountryId);
            if (country == null)
                return BadRequest(new { message = "Страна не найдена" });

            university.Name = dto.Name;
            university.NameEn = dto.NameEn;
            university.City = dto.City;
            university.Description = dto.Description;
            university.Website = dto.Website;
            university.Type = (UniversityType)dto.Type;
            university.CountryId = dto.CountryId;
            university.IsActive = dto.IsActive;

            // Обновляем типы экзаменов
            university.ExamTypes.Clear();
            if (dto.ExamTypeIds != null && dto.ExamTypeIds.Any())
            {
                var examTypes = await _context.ExamTypes
                    .Where(et => dto.ExamTypeIds.Contains(et.Id))
                    .ToListAsync();
                foreach (var examType in examTypes)
                {
                    university.ExamTypes.Add(examType);
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Удалить университет (только админ)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUniversity(int id)
        {
            var university = await _context.Universities
                .Include(u => u.Exams)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (university == null)
                return NotFound();

            if (university.Exams.Any())
                return BadRequest(new { message = "Невозможно удалить университет, к которому привязаны экзамены" });

            _context.Universities.Remove(university);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
