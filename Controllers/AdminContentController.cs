using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

[ApiController]
[Route("api/admin/content")]
[Authorize(Roles = "Admin")]
public class AdminContentController : ControllerBase
{
    private readonly UniStartDbContext _db;

    public AdminContentController(UniStartDbContext db)
    {
        _db = db;
    }

    // ══════════════════════════════════════════════
    //  LESSONS
    // ══════════════════════════════════════════════

    [HttpGet("lessons")]
    public async Task<IActionResult> GetLessons([FromQuery] int? topicId)
    {
        var query = _db.Set<TopicLesson>()
            .Include(l => l.Topic)
            .Include(l => l.Steps)
            .AsNoTracking();

        if (topicId.HasValue)
            query = query.Where(l => l.TopicId == topicId.Value);

        var items = await query.OrderBy(l => l.TopicId).ThenBy(l => l.SortOrder).ToListAsync();

        return Ok(items.Select(l => new AdminLessonListDto(
            l.Id, l.TopicId, l.Topic.Name, l.Title, l.VideoUrl, l.SortOrder, l.Steps.Count
        )));
    }

    [HttpPost("lessons")]
    public async Task<IActionResult> CreateLesson([FromBody] AdminCreateLessonDto dto)
    {
        var topic = await _db.Topics.FindAsync(dto.TopicId);
        if (topic == null) return BadRequest(new { error = "Тема не найдена" });

        var lesson = new TopicLesson
        {
            TopicId = dto.TopicId,
            Title = dto.Title,
            Content = dto.Content,
            VideoUrl = dto.VideoUrl,
            SortOrder = dto.SortOrder,
        };
        _db.Set<TopicLesson>().Add(lesson);
        await _db.SaveChangesAsync();

        return Ok(new AdminLessonListDto(lesson.Id, lesson.TopicId, topic.Name, lesson.Title, lesson.VideoUrl, lesson.SortOrder, 0));
    }

    [HttpPut("lessons/{id}")]
    public async Task<IActionResult> UpdateLesson(int id, [FromBody] AdminUpdateLessonDto dto)
    {
        var lesson = await _db.Set<TopicLesson>().FindAsync(id);
        if (lesson == null) return NotFound();

        if (dto.Title != null) lesson.Title = dto.Title;
        if (dto.Content != null) lesson.Content = dto.Content;
        if (dto.VideoUrl != null) lesson.VideoUrl = dto.VideoUrl;
        if (dto.SortOrder.HasValue) lesson.SortOrder = dto.SortOrder.Value;

        await _db.SaveChangesAsync();
        return Ok(new { id = lesson.Id });
    }

    [HttpDelete("lessons/{id}")]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        var lesson = await _db.Set<TopicLesson>().FindAsync(id);
        if (lesson == null) return NotFound();

        _db.Set<TopicLesson>().Remove(lesson);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }

    // ═══════════════════════════════════════════════
    //  FLASHCARD DECKS
    // ═══════════════════════════════════════════════

    [HttpGet("decks")]
    public async Task<IActionResult> GetDecks([FromQuery] string? examTypeCode)
    {
        var query = _db.Set<FlashcardDeck>()
            .Include(d => d.Cards)
            .Include(d => d.Topic)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(examTypeCode))
            query = query.Where(d => d.ExamTypeCode == examTypeCode);

        var items = await query.OrderBy(d => d.ExamTypeCode).ThenBy(d => d.Title).ToListAsync();

        return Ok(items.Select(d => new AdminDeckListDto(
            d.Id, d.Title, d.Description, d.ExamTypeCode,
            d.TopicId, d.Topic?.Name, d.IsSystem, d.Cards.Count, d.CreatedAt
        )));
    }

    [HttpPost("decks")]
    public async Task<IActionResult> CreateDeck([FromBody] AdminCreateDeckDto dto)
    {
        var deck = new FlashcardDeck
        {
            Title = dto.Title,
            Description = dto.Description,
            ExamTypeCode = dto.ExamTypeCode,
            TopicId = dto.TopicId,
            IsSystem = true,
        };
        _db.Set<FlashcardDeck>().Add(deck);
        await _db.SaveChangesAsync();
        return Ok(new { id = deck.Id, title = deck.Title });
    }

    [HttpPut("decks/{id}")]
    public async Task<IActionResult> UpdateDeck(int id, [FromBody] AdminUpdateDeckDto dto)
    {
        var deck = await _db.Set<FlashcardDeck>().FindAsync(id);
        if (deck == null) return NotFound();

        if (dto.Title != null) deck.Title = dto.Title;
        if (dto.Description != null) deck.Description = dto.Description;
        if (dto.ExamTypeCode != null) deck.ExamTypeCode = dto.ExamTypeCode;
        if (dto.TopicId.HasValue) deck.TopicId = dto.TopicId;
        deck.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { id = deck.Id });
    }

    [HttpDelete("decks/{id}")]
    public async Task<IActionResult> DeleteDeck(int id)
    {
        var deck = await _db.Set<FlashcardDeck>().Include(d => d.Cards).FirstOrDefaultAsync(d => d.Id == id);
        if (deck == null) return NotFound();

        _db.Set<FlashcardDeck>().Remove(deck);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }

    // ─── Flashcards (inside a deck) ────────────────

    [HttpGet("decks/{deckId}/cards")]
    public async Task<IActionResult> GetCards(int deckId)
    {
        var cards = await _db.Set<Flashcard>()
            .Where(c => c.DeckId == deckId)
            .OrderBy(c => c.SortOrder)
            .AsNoTracking()
            .ToListAsync();

        return Ok(cards.Select(c => new AdminFlashcardDto(c.Id, c.DeckId, c.Front, c.Back, c.SortOrder)));
    }

    [HttpPost("cards")]
    public async Task<IActionResult> CreateCard([FromBody] AdminCreateFlashcardDto dto)
    {
        var deck = await _db.Set<FlashcardDeck>().FindAsync(dto.DeckId);
        if (deck == null) return BadRequest(new { error = "Колода не найдена" });

        var card = new Flashcard
        {
            DeckId = dto.DeckId,
            Front = dto.Front,
            Back = dto.Back,
            SortOrder = dto.SortOrder,
        };
        _db.Set<Flashcard>().Add(card);
        await _db.SaveChangesAsync();
        return Ok(new AdminFlashcardDto(card.Id, card.DeckId, card.Front, card.Back, card.SortOrder));
    }

    [HttpPut("cards/{id}")]
    public async Task<IActionResult> UpdateCard(int id, [FromBody] AdminUpdateFlashcardDto dto)
    {
        var card = await _db.Set<Flashcard>().FindAsync(id);
        if (card == null) return NotFound();

        if (dto.Front != null) card.Front = dto.Front;
        if (dto.Back != null) card.Back = dto.Back;
        if (dto.SortOrder.HasValue) card.SortOrder = dto.SortOrder.Value;
        card.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { id = card.Id });
    }

    [HttpDelete("cards/{id}")]
    public async Task<IActionResult> DeleteCard(int id)
    {
        var card = await _db.Set<Flashcard>().FindAsync(id);
        if (card == null) return NotFound();

        _db.Set<Flashcard>().Remove(card);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }

    // ═══════════════════════════════════════════════
    //  FORMULA CARDS
    // ═══════════════════════════════════════════════

    [HttpGet("formulas")]
    public async Task<IActionResult> GetFormulas([FromQuery] int? topicId)
    {
        var query = _db.Set<FormulaCard>()
            .Include(f => f.Topic)
            .AsNoTracking();

        if (topicId.HasValue)
            query = query.Where(f => f.TopicId == topicId.Value);

        var items = await query.OrderBy(f => f.TopicId).ThenBy(f => f.SortOrder).ToListAsync();

        return Ok(items.Select(f => new AdminFormulaListDto(
            f.Id, f.TopicId, f.Topic.Name, f.Title, f.Formula, f.Description, f.SortOrder
        )));
    }

    [HttpPost("formulas")]
    public async Task<IActionResult> CreateFormula([FromBody] AdminCreateFormulaDto dto)
    {
        var topic = await _db.Topics.FindAsync(dto.TopicId);
        if (topic == null) return BadRequest(new { error = "Тема не найдена" });

        var formula = new FormulaCard
        {
            TopicId = dto.TopicId,
            Title = dto.Title,
            Formula = dto.Formula,
            Description = dto.Description,
            SortOrder = dto.SortOrder,
        };
        _db.Set<FormulaCard>().Add(formula);
        await _db.SaveChangesAsync();

        return Ok(new AdminFormulaListDto(formula.Id, formula.TopicId, topic.Name, formula.Title, formula.Formula, formula.Description, formula.SortOrder));
    }

    [HttpPut("formulas/{id}")]
    public async Task<IActionResult> UpdateFormula(int id, [FromBody] AdminUpdateFormulaDto dto)
    {
        var formula = await _db.Set<FormulaCard>().FindAsync(id);
        if (formula == null) return NotFound();

        if (dto.Title != null) formula.Title = dto.Title;
        if (dto.Formula != null) formula.Formula = dto.Formula;
        if (dto.Description != null) formula.Description = dto.Description;
        if (dto.SortOrder.HasValue) formula.SortOrder = dto.SortOrder.Value;
        formula.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { id = formula.Id });
    }

    [HttpDelete("formulas/{id}")]
    public async Task<IActionResult> DeleteFormula(int id)
    {
        var formula = await _db.Set<FormulaCard>().FindAsync(id);
        if (formula == null) return NotFound();

        _db.Set<FormulaCard>().Remove(formula);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }

    // ═══════════════════════════════════════════════
    //  STRATEGY GUIDES
    // ═══════════════════════════════════════════════

    [HttpGet("strategies")]
    public async Task<IActionResult> GetStrategies([FromQuery] string? examTypeCode)
    {
        var query = _db.Set<StrategyGuide>().AsNoTracking();

        if (!string.IsNullOrEmpty(examTypeCode))
            query = query.Where(s => s.ExamTypeCode == examTypeCode);

        var items = await query.OrderBy(s => s.ExamTypeCode).ThenBy(s => s.SortOrder).ToListAsync();

        return Ok(items.Select(s => new AdminStrategyListDto(
            s.Id, s.ExamTypeCode, s.Title, s.Summary, s.Category, s.EstimatedReadMinutes, s.SortOrder
        )));
    }

    [HttpGet("strategies/{id}")]
    public async Task<IActionResult> GetStrategy(int id)
    {
        var s = await _db.Set<StrategyGuide>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return NotFound();

        return Ok(new { s.Id, s.ExamTypeCode, s.Title, s.Summary, s.Content, s.Category, s.EstimatedReadMinutes, s.SortOrder });
    }

    [HttpPost("strategies")]
    public async Task<IActionResult> CreateStrategy([FromBody] AdminCreateStrategyDto dto)
    {
        var strategy = new StrategyGuide
        {
            ExamTypeCode = dto.ExamTypeCode,
            Title = dto.Title,
            Summary = dto.Summary,
            Content = dto.Content,
            Category = dto.Category,
            EstimatedReadMinutes = dto.EstimatedReadMinutes,
            SortOrder = dto.SortOrder,
        };
        _db.Set<StrategyGuide>().Add(strategy);
        await _db.SaveChangesAsync();

        return Ok(new AdminStrategyListDto(strategy.Id, strategy.ExamTypeCode, strategy.Title, strategy.Summary, strategy.Category, strategy.EstimatedReadMinutes, strategy.SortOrder));
    }

    [HttpPut("strategies/{id}")]
    public async Task<IActionResult> UpdateStrategy(int id, [FromBody] AdminUpdateStrategyDto dto)
    {
        var strategy = await _db.Set<StrategyGuide>().FindAsync(id);
        if (strategy == null) return NotFound();

        if (dto.Title != null) strategy.Title = dto.Title;
        if (dto.Summary != null) strategy.Summary = dto.Summary;
        if (dto.Content != null) strategy.Content = dto.Content;
        if (dto.Category != null) strategy.Category = dto.Category;
        if (dto.EstimatedReadMinutes.HasValue) strategy.EstimatedReadMinutes = dto.EstimatedReadMinutes.Value;
        if (dto.SortOrder.HasValue) strategy.SortOrder = dto.SortOrder.Value;
        strategy.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { id = strategy.Id });
    }

    [HttpDelete("strategies/{id}")]
    public async Task<IActionResult> DeleteStrategy(int id)
    {
        var strategy = await _db.Set<StrategyGuide>().FindAsync(id);
        if (strategy == null) return NotFound();

        _db.Set<StrategyGuide>().Remove(strategy);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }

    // ═══════════════════════════════════════════════
    //  DRILL TEMPLATES
    // ═══════════════════════════════════════════════

    [HttpGet("drills")]
    public async Task<IActionResult> GetDrills()
    {
        var items = await _db.DrillTemplates
            .Include(d => d.Topic)
            .AsNoTracking()
            .OrderBy(d => d.SortOrder).ThenBy(d => d.Title)
            .ToListAsync();

        return Ok(items.Select(d => new AdminDrillTemplateListDto(
            d.Id, d.Title, d.Description, d.DrillType.ToString(),
            d.ExamTypeCode, d.TopicId, d.Topic?.Name,
            d.QuestionCount, d.TimeLimitMinutes, d.IsActive, d.SortOrder
        )));
    }

    [HttpPost("drills")]
    public async Task<IActionResult> CreateDrill([FromBody] AdminCreateDrillTemplateDto dto)
    {
        if (!Enum.TryParse<DrillType>(dto.DrillType, true, out var drillType))
            return BadRequest(new { error = "Неверный тип дрилла" });

        var drill = new DrillTemplate
        {
            Title = dto.Title,
            Description = dto.Description,
            DrillType = drillType,
            ExamTypeCode = dto.ExamTypeCode,
            TopicId = dto.TopicId,
            QuestionCount = dto.QuestionCount,
            TimeLimitMinutes = dto.TimeLimitMinutes,
            IsActive = dto.IsActive,
            SortOrder = dto.SortOrder,
        };
        _db.DrillTemplates.Add(drill);
        await _db.SaveChangesAsync();

        return Ok(new AdminDrillTemplateListDto(
            drill.Id, drill.Title, drill.Description, drill.DrillType.ToString(),
            drill.ExamTypeCode, drill.TopicId, null,
            drill.QuestionCount, drill.TimeLimitMinutes, drill.IsActive, drill.SortOrder
        ));
    }

    [HttpPut("drills/{id}")]
    public async Task<IActionResult> UpdateDrill(int id, [FromBody] AdminUpdateDrillTemplateDto dto)
    {
        var drill = await _db.DrillTemplates.FindAsync(id);
        if (drill == null) return NotFound();

        if (dto.Title != null) drill.Title = dto.Title;
        if (dto.Description != null) drill.Description = dto.Description;
        if (dto.DrillType != null)
        {
            if (!Enum.TryParse<DrillType>(dto.DrillType, true, out var drillType))
                return BadRequest(new { error = "Неверный тип дрилла" });
            drill.DrillType = drillType;
        }
        if (dto.ExamTypeCode != null) drill.ExamTypeCode = dto.ExamTypeCode == "" ? null : dto.ExamTypeCode;
        if (dto.TopicId.HasValue) drill.TopicId = dto.TopicId.Value == 0 ? null : dto.TopicId.Value;
        if (dto.QuestionCount.HasValue) drill.QuestionCount = dto.QuestionCount.Value;
        if (dto.TimeLimitMinutes.HasValue) drill.TimeLimitMinutes = dto.TimeLimitMinutes.Value == 0 ? null : dto.TimeLimitMinutes.Value;
        if (dto.IsActive.HasValue) drill.IsActive = dto.IsActive.Value;
        if (dto.SortOrder.HasValue) drill.SortOrder = dto.SortOrder.Value;
        drill.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { id = drill.Id });
    }

    [HttpDelete("drills/{id}")]
    public async Task<IActionResult> DeleteDrill(int id)
    {
        var drill = await _db.DrillTemplates.FindAsync(id);
        if (drill == null) return NotFound();

        _db.DrillTemplates.Remove(drill);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true });
    }
}
