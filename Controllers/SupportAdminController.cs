using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

[ApiController]
[Route("api/admin/support")]
[Authorize(Roles = "Admin")]
public class SupportAdminController : ControllerBase
{
    private readonly UniStartDbContext _db;
    private readonly ITelegramBotService _bot;

    public SupportAdminController(UniStartDbContext db, ITelegramBotService bot)
    {
        _db = db;
        _bot = bot;
    }

    [HttpGet("tickets")]
    public async Task<IActionResult> Tickets()
    {
        var tickets = await _db.SupportTickets
            .OrderByDescending(t => t.LastMessageAt)
            .Select(t => new
            {
                t.Id,
                t.TelegramUserId,
                t.Username,
                t.FirstName,
                t.Status,
                t.LastMessageAt,
                t.CreatedAt,
                MessageCount = t.Messages.Count,
            })
            .ToListAsync();
        return Ok(tickets);
    }

    [HttpGet("tickets/{id:int}")]
    public async Task<IActionResult> Ticket(int id)
    {
        var ticket = await _db.SupportTickets
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == id);
        if (ticket == null) return NotFound();

        return Ok(new
        {
            ticket.Id,
            ticket.TelegramUserId,
            ticket.Username,
            ticket.FirstName,
            ticket.Status,
            ticket.LastMessageAt,
            Messages = ticket.Messages.Select(m => new { m.Id, m.Direction, m.Text, m.CreatedAt }),
        });
    }

    [HttpPost("tickets/{id:int}/reply")]
    public async Task<IActionResult> Reply(int id, [FromBody] SupportReplyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text)) return BadRequest(new { error = "Text is required" });
        var ok = await _bot.SendOperatorReplyAsync(id, dto.Text.Trim());
        return ok ? Ok() : StatusCode(502, new { error = "Failed to send reply" });
    }

    public record SupportReplyDto(string Text);
}
