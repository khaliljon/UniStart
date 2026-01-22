using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.DTOs;

namespace UniStart.Controllers.Reference
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить все города (можно фильтровать по стране)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CityDto>>> GetCities([FromQuery] int? countryId = null)
        {
            var query = _context.Cities
                .Include(c => c.Country)
                .Where(c => c.IsActive);

            if (countryId.HasValue)
                query = query.Where(c => c.CountryId == countryId.Value);

            var cities = await query
                .OrderBy(c => c.Name)
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    NameEn = c.NameEn,
                    CountryId = c.CountryId,
                    CountryName = c.Country.Name,
                    CountryCode = c.Country.Code,
                    Region = c.Region,
                    Population = c.Population,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(cities);
        }

        /// <summary>
        /// Получить город по ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CityDto>> GetCity(int id)
        {
            var city = await _context.Cities
                .Include(c => c.Country)
                .Where(c => c.Id == id)
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    NameEn = c.NameEn,
                    CountryId = c.CountryId,
                    CountryName = c.Country.Name,
                    CountryCode = c.Country.Code,
                    Region = c.Region,
                    Population = c.Population,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (city == null)
                return NotFound();

            return Ok(city);
        }

        /// <summary>
        /// Создать город (только админ)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCity([FromBody] CreateCityDto dto)
        {
            var country = await _context.Countries.FindAsync(dto.CountryId);
            if (country == null)
                return BadRequest(new { message = "Страна не найдена" });

            var city = new Models.Reference.City
            {
                Name = dto.Name,
                NameEn = dto.NameEn,
                CountryId = dto.CountryId,
                Region = dto.Region,
                Population = dto.Population,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCity), new { id = city.Id }, city);
        }

        /// <summary>
        /// Обновить город (только админ)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCity(int id, [FromBody] UpdateCityDto dto)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                return NotFound();

            var country = await _context.Countries.FindAsync(dto.CountryId);
            if (country == null)
                return BadRequest(new { message = "Страна не найдена" });

            city.Name = dto.Name;
            city.NameEn = dto.NameEn;
            city.CountryId = dto.CountryId;
            city.Region = dto.Region;
            city.Population = dto.Population;
            city.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Удалить город (только админ)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCity(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
                return NotFound();

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
