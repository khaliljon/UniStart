using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TagsController> _logger;

    public TagsController(ApplicationDbContext context, ILogger<TagsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Получить все теги
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Tag>>> GetAllTags([FromQuery] string? search = null)
    {
        var query = _context.Tags.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Name.Contains(search));

        var tags = await query
            .OrderBy(t => t.Name)
            .ToListAsync();

        return Ok(tags);
    }

    /// <summary>
    /// Получить популярные теги
    /// </summary>
    [HttpGet("popular")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Tag>>> GetPopularTags([FromQuery] int count = 10)
    {
        var tags = await _context.Tags
            .OrderBy(t => t.Name)
            .Take(count)
            .ToListAsync();

        return Ok(tags);
    }

    /// <summary>
    /// Создать новый тег
    /// </summary>
    [HttpPost]
    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Teacher}")]
    public async Task<ActionResult<Tag>> CreateTag([FromBody] CreateTagDto dto)
    {
        // Проверяем, существует ли тег
        var existingTag = await _context.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == dto.Name.ToLower());

        if (existingTag != null)
            return BadRequest(new { Message = "Тег с таким названием уже существует" });

        var tag = new Tag
        {
            Name = dto.Name,
            Color = dto.Color ?? "#3B82F6"
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Создан новый тег: {TagName}", tag.Name);

        return CreatedAtAction(nameof(GetAllTags), new { id = tag.Id }, tag);
    }

    /// <summary>
    /// Удалить тег (только админы)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<ActionResult> DeleteTag(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null)
            return NotFound(new { Message = "Тег не найден" });

        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Удалён тег: {TagName}", tag.Name);

        return NoContent();
    }

    /// <summary>
    /// Получить квизы по тегу
    /// </summary>
    [HttpGet("{tagId}/quizzes")]
    public async Task<ActionResult<object>> GetQuizzesByTag(int tagId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var tag = await _context.Tags
            .Include(t => t.Quizzes)
                .ThenInclude(q => q.Questions)
            .FirstOrDefaultAsync(t => t.Id == tagId);

        if (tag == null)
            return NotFound(new { Message = "Тег не найден" });

        var quizzes = tag.Quizzes
            .Where(q => q.IsPublic)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new
            {
                q.Id,
                q.Title,
                q.Description,
                q.Subject,
                q.Difficulty,
                QuestionCount = q.Questions.Count
            })
            .ToList();

        return Ok(new
        {
            Tag = new { tag.Id, tag.Name, tag.Color },
            Quizzes = quizzes
        });
    }

    /// <summary>
    /// Получить наборы карточек по тегу
    /// </summary>
    [HttpGet("{tagId}/flashcard-sets")]
    public async Task<ActionResult<object>> GetFlashcardSetsByTag(int tagId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var tag = await _context.Tags
            .Include(t => t.FlashcardSets)
                .ThenInclude(fs => fs.Flashcards)
            .FirstOrDefaultAsync(t => t.Id == tagId);

        if (tag == null)
            return NotFound(new { Message = "Тег не найден" });

        var flashcardSets = tag.FlashcardSets
            .Where(fs => fs.IsPublic)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(fs => new
            {
                fs.Id,
                fs.Title,
                fs.Description,
                fs.Subject,
                CardCount = fs.Flashcards.Count
            })
            .ToList();

        return Ok(new
        {
            Tag = new { tag.Id, tag.Name, tag.Color },
            FlashcardSets = flashcardSets
        });
    }
}

// DTOs
public class CreateTagDto
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}
