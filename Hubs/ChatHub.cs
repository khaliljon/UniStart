using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UniStart.Application.Interfaces;
using UniStart.Application.Services;
using UniStart.Infrastructure.Data;

namespace UniStart.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly PresenceTracker _presenceTracker;
    private readonly UniStartDbContext _db;

    public ChatHub(IMessageService messageService, PresenceTracker presenceTracker, UniStartDbContext db)
    {
        _messageService = messageService;
        _presenceTracker = presenceTracker;
        _db = db;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            var isNew = _presenceTracker.UserConnected(userId, Context.ConnectionId);

            // Update LastSeenAt in DB
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastSeenAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            // If user just came online, broadcast to others who have conversations with them
            if (isNew)
            {
                await Clients.Others.SendAsync("UserOnline", userId);
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

            var isOffline = _presenceTracker.UserDisconnected(userId, Context.ConnectionId);

            // Update LastSeenAt in DB
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastSeenAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            if (isOffline)
            {
                await Clients.Others.SendAsync("UserOffline", userId);
            }
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

        // Send with correct IsMine flag for each participant
        // Use Clients.Caller for sender to prevent duplication if orphaned connections exist
        var recipientMessage = new UniStart.Application.DTOs.MessageDto(
            message.Id, message.ConversationId, message.SenderId, message.SenderName,
            message.Text, message.SentAt, message.ReadAt, message.IsEdited,
            message.Type, IsMine: false
        );

        await Clients.Caller.SendAsync("ReceiveMessage", message);
        await Clients.Group($"user_{recipientId}").SendAsync("ReceiveMessage", recipientMessage);

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
            // Send userName (not userId) so clients can display a name
            var user = await _messageService.GetUserNameAsync(userId);
            await Clients.Group($"user_{conv.OtherUserId}").SendAsync("UserTyping", conversationId, user);
        }
    }

    private int GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? Context.User?.FindFirst("sub")?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }
}
