using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UniStart.Application.Interfaces;

namespace UniStart.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;

    public ChatHub(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Send a message through the hub. Persists to DB and pushes to both participants.
    /// </summary>
    public async Task SendMessage(int conversationId, string text)
    {
        var userId = GetUserId();
        if (userId <= 0 || string.IsNullOrWhiteSpace(text)) return;

        var message = await _messageService.SendMessageAsync(userId, conversationId, text);

        // Get conversation to find recipient
        var conversations = await _messageService.GetConversationsAsync(userId);
        var conv = conversations.FirstOrDefault(c => c.Id == conversationId);
        if (conv == null) return;

        var recipientId = conv.OtherUserId;

        // Push to both sender and recipient
        await Clients.Group($"user_{userId}").SendAsync("ReceiveMessage", conversationId, message);
        await Clients.Group($"user_{recipientId}").SendAsync("ReceiveMessage", conversationId, message);

        // Update unread count for recipient
        var unread = await _messageService.GetUnreadCountAsync(recipientId);
        await Clients.Group($"user_{recipientId}").SendAsync("UnreadCountUpdate", unread);
    }

    /// <summary>
    /// Mark conversation messages as read. Pushes read receipts.
    /// </summary>
    public async Task MarkAsRead(int conversationId)
    {
        var userId = GetUserId();
        if (userId <= 0) return;

        await _messageService.MarkAsReadAsync(conversationId, userId);

        // Notify sender that messages were read
        var conversations = await _messageService.GetConversationsAsync(userId);
        var conv = conversations.FirstOrDefault(c => c.Id == conversationId);
        if (conv != null)
        {
            await Clients.Group($"user_{conv.OtherUserId}").SendAsync("MessagesRead", conversationId);
        }

        // Update own unread count
        var unread = await _messageService.GetUnreadCountAsync(userId);
        await Clients.Group($"user_{userId}").SendAsync("UnreadCountUpdate", unread);
    }

    /// <summary>
    /// Typing indicator. Ephemeral — not persisted.
    /// </summary>
    public async Task Typing(int conversationId)
    {
        var userId = GetUserId();
        if (userId <= 0) return;

        var conversations = await _messageService.GetConversationsAsync(userId);
        var conv = conversations.FirstOrDefault(c => c.Id == conversationId);
        if (conv != null)
        {
            await Clients.Group($"user_{conv.OtherUserId}").SendAsync("UserTyping", conversationId, userId);
        }
    }

    private int GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? Context.User?.FindFirst("sub")?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }
}
