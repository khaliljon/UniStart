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
    public class CountriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CountriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить все страны
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CountryDto>>> GetCountries()
        {
            var countries = await _context.Countries
                .Include(c => c.Universities)
                .Include(c => c.Exams)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new CountryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    NameEn = c.NameEn,
                    Code = c.Code,
                    FlagEmoji = c.FlagEmoji,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UniversitiesCount = c.Universities.Count,
                    ExamsCount = c.Exams.Count
                })
                .ToListAsync();

            return Ok(countries);
        }

        /// <summary>
        /// Получить страну по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            var country = await _context.Countries
                .Include(c => c.Universities)
                .Include(c => c.Exams)
                .Where(c => c.Id == id)
                .Select(c => new CountryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    NameEn = c.NameEn,
                    Code = c.Code,
                    FlagEmoji = c.FlagEmoji,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UniversitiesCount = c.Universities.Count,
                    ExamsCount = c.Exams.Count
                })
                .FirstOrDefaultAsync();

            if (country == null)
                return NotFound();

            return Ok(country);
        }

        /// <summary>
        /// Создать страну (только админ)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Country>> CreateCountry(CreateCountryDto dto)
        {
            var country = new Country
            {
                Name = dto.Name,
                NameEn = dto.NameEn,
                Code = dto.Code.ToUpper(),
                FlagEmoji = dto.FlagEmoji,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, country);
        }

        /// <summary>
        /// Обновить страну (только админ)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCountry(int id, UpdateCountryDto dto)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
                return NotFound();

            country.Name = dto.Name;
            country.NameEn = dto.NameEn;
            country.Code = dto.Code.ToUpper();
            country.FlagEmoji = dto.FlagEmoji;
            country.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Удалить страну (только админ)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _context.Countries
                .Include(c => c.Universities)
                .Include(c => c.Exams)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (country == null)
                return NotFound();

            if (country.Universities.Any() || country.Exams.Any())
                return BadRequest(new { message = "Невозможно удалить страну, к которой привязаны университеты или экзамены" });

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
