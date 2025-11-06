using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExportImportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExportImportController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>
        /// Экспорт набора флешкарт в JSON
        /// </summary>
        [HttpGet("flashcards/{setId}/export/json")]
        public async Task<IActionResult> ExportFlashcardsJson(int setId)
        {
            var userId = GetUserId();
            
            var set = await _context.FlashcardSets
                .Where(fs => fs.Id == setId && fs.UserId == userId)
                .Include(fs => fs.Flashcards)
                .FirstOrDefaultAsync();

            if (set == null)
                return NotFound("FlashcardSet not found or access denied");

            var exportData = new
            {
                set.Title,
                set.Description,
                ExportedAt = DateTime.UtcNow,
                Flashcards = set.Flashcards.Select(f => new
                {
                    f.Question,
                    f.Answer,
                    f.Explanation,
                    f.OrderIndex
                }).ToList()
            };

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(json);

            return File(bytes, "application/json", $"{set.Title}.json");
        }

        /// <summary>
        /// Экспорт набора флешкарт в CSV
        /// </summary>
        [HttpGet("flashcards/{setId}/export/csv")]
        public async Task<IActionResult> ExportFlashcardsCsv(int setId)
        {
            var userId = GetUserId();
            
            var set = await _context.FlashcardSets
                .Where(fs => fs.Id == setId && fs.UserId == userId)
                .Include(fs => fs.Flashcards)
                .FirstOrDefaultAsync();

            if (set == null)
                return NotFound("FlashcardSet not found or access denied");

            var csv = new StringBuilder();
            csv.AppendLine("Question,Answer,Explanation");

            foreach (var card in set.Flashcards.OrderBy(f => f.OrderIndex))
            {
                csv.AppendLine($"\"{card.Question}\",\"{card.Answer}\",\"{card.Explanation}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"{set.Title}.csv");
        }

        /// <summary>
        /// Импорт флешкарт из JSON
        /// </summary>
        [HttpPost("flashcards/import/json")]
        public async Task<IActionResult> ImportFlashcardsJson([FromBody] ImportFlashcardsDto dto)
        {
            var userId = GetUserId();

            var set = new FlashcardSet
            {
                Title = dto.Title,
                Description = dto.Description,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.FlashcardSets.Add(set);
            await _context.SaveChangesAsync();

            var flashcards = dto.Flashcards.Select((f, index) => new Flashcard
            {
                Question = f.Question,
                Answer = f.Answer,
                Explanation = f.Explanation ?? "",
                FlashcardSetId = set.Id,
                OrderIndex = index
            }).ToList();

            _context.Flashcards.AddRange(flashcards);
            await _context.SaveChangesAsync();

            return Ok(new { SetId = set.Id, ImportedCount = flashcards.Count });
        }
    }

    public class ImportFlashcardsDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ImportFlashcardItemDto> Flashcards { get; set; } = new();
    }

    public class ImportFlashcardItemDto
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string? Explanation { get; set; }
    }
}
