using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
[ApiVersion("1.0")]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    /// <summary>Все диалоги текущего пользователя</summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = GetUserId();
        var conversations = await _messageService.GetConversationsAsync(userId);
        return Ok(conversations);
    }

    /// <summary>Сообщения конкретного диалога</summary>
    [HttpGet("conversations/{id:int}")]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var userId = GetUserId();
        var messages = await _messageService.GetMessagesAsync(id, userId, page, pageSize);
        return Ok(messages);
    }

    /// <summary>Начать диалог с тьютором</summary>
    [HttpPost("conversations")]
    public async Task<IActionResult> StartConversation([FromBody] StartConversationDto dto)
    {
        var userId = GetUserId();
        var conversation = await _messageService.StartConversationAsync(userId, dto.TutorId, dto.Message);
        return Ok(conversation);
    }

    /// <summary>Отправить сообщение (REST fallback, основной путь — SignalR)</summary>
    [HttpPost("conversations/{id:int}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageDto dto)
    {
        var userId = GetUserId();
        var message = await _messageService.SendMessageAsync(userId, id, dto.Text);
        return Ok(message);
    }

    /// <summary>Пометить диалог прочитанным</summary>
    [HttpPost("conversations/{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetUserId();
        await _messageService.MarkAsReadAsync(id, userId);
        return Ok();
    }

    /// <summary>Общее кол-во непрочитанных сообщений</summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _messageService.GetUnreadCountAsync(userId);
        return Ok(count);
    }

    /// <summary>Архивировать диалог</summary>
    [HttpPost("conversations/{id:int}/archive")]
    public async Task<IActionResult> ArchiveConversation(int id)
    {
        var userId = GetUserId();
        var result = await _messageService.ArchiveConversationAsync(id, userId);
        return result ? Ok(new { archived = true }) : NotFound();
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        if (int.TryParse(claim, out var id)) return id;
        throw new UnauthorizedAccessException("User ID not found in token");
    }
}
