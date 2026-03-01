using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class MessageService : IMessageService
{
    private readonly UniStartDbContext _db;

    public MessageService(UniStartDbContext db)
    {
        _db = db;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(int userId)
    {
        var conversations = await _db.Conversations
            .Include(c => c.Student)
            .Include(c => c.Tutor)
            .Where(c => c.StudentId == userId || c.TutorId == userId)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync();

        return conversations.Select(c =>
        {
            var isStudent = c.StudentId == userId;
            var other = isStudent ? c.Tutor : c.Student;
            var unread = isStudent ? c.UnreadCountStudent : c.UnreadCountTutor;

            return new ConversationDto(
                c.Id,
                other.Id,
                other.Name,
                other.Role.ToString(),
                c.LastMessagePreview,
                c.LastMessageAt,
                unread,
                c.Status.ToString(),
                c.RequestMessage
            );
        }).ToList();
    }

    public async Task<MessagesPageDto> GetMessagesAsync(int conversationId, int userId, int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(1, page);

        var conv = await _db.Conversations.FindAsync(conversationId)
            ?? throw new KeyNotFoundException("Conversation not found");

        if (conv.StudentId != userId && conv.TutorId != userId)
            throw new UnauthorizedAccessException("Access denied");

        var totalCount = await _db.Messages
            .CountAsync(m => m.ConversationId == conversationId);

        var messages = await _db.Messages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = messages.Select(m => new MessageDto(
            m.Id,
            conversationId,
            m.SenderId,
            m.Sender.Name,
            m.Text,
            m.SentAt,
            m.ReadAt,
            m.IsEdited,
            m.Type.ToString(),
            m.SenderId == userId
        )).ToList();

        return new MessagesPageDto(items, totalCount, totalCount > page * pageSize);
    }

    public async Task<MessageDto> SendMessageAsync(int senderId, int conversationId, string text)
    {
        var conv = await _db.Conversations.FindAsync(conversationId)
            ?? throw new KeyNotFoundException("Conversation not found");

        if (conv.StudentId != senderId && conv.TutorId != senderId)
            throw new UnauthorizedAccessException("Access denied");

        // Block sending in non-active conversations
        if (conv.Status != ConversationStatus.Active)
            throw new InvalidOperationException("Отправка сообщений возможна только в активных диалогах");

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = senderId,
            Text = text.Length > 4000 ? text[..4000] : text,
            SentAt = DateTime.UtcNow,
            Type = MessageType.Text
        };

        _db.Messages.Add(message);

        // Update conversation metadata
        conv.LastMessagePreview = text.Length > 100 ? text[..100] : text;
        conv.LastMessageAt = message.SentAt;

        // Increment unread for the other party
        if (conv.StudentId == senderId)
            conv.UnreadCountTutor++;
        else
            conv.UnreadCountStudent++;

        await _db.SaveChangesAsync();

        var sender = await _db.Users.FindAsync(senderId);
        return new MessageDto(
            message.Id, conversationId, senderId, sender?.Name ?? "—", message.Text,
            message.SentAt, null, false, "Text", true
        );
    }

    public async Task<ConversationDto> StartConversationAsync(int studentId, int tutorId, string? requestMessage)
    {
        // Check tutor exists and is a tutor
        var tutor = await _db.Users.FindAsync(tutorId)
            ?? throw new KeyNotFoundException("Tutor not found");
        if (tutor.Role != UserRole.Tutor)
            throw new ArgumentException("User is not a tutor");

        var student = await _db.Users.FindAsync(studentId)
            ?? throw new KeyNotFoundException("Student not found");

        // Check for existing conversation
        var existing = await _db.Conversations
            .Include(c => c.Student)
            .Include(c => c.Tutor)
            .FirstOrDefaultAsync(c => c.StudentId == studentId && c.TutorId == tutorId);

        if (existing != null)
        {
            return new ConversationDto(
                existing.Id, tutor.Id, tutor.Name, tutor.Role.ToString(),
                existing.LastMessagePreview, existing.LastMessageAt,
                existing.UnreadCountStudent, existing.Status.ToString(),
                existing.RequestMessage
            );
        }

        // Create new conversation with Pending status
        var conv = new Conversation
        {
            StudentId = studentId,
            TutorId = tutorId,
            Status = ConversationStatus.Pending,
            RequestMessage = requestMessage?.Length > 500 ? requestMessage[..500] : requestMessage,
            CreatedAt = DateTime.UtcNow
        };

        _db.Conversations.Add(conv);

        // System message
        var sysMsg = new Message
        {
            Conversation = conv,
            SenderId = studentId,
            Text = $"{student.Name} отправил(а) заявку на обучение",
            SentAt = DateTime.UtcNow,
            Type = MessageType.System
        };
        _db.Messages.Add(sysMsg);

        conv.LastMessagePreview = sysMsg.Text;
        conv.LastMessageAt = sysMsg.SentAt;
        conv.UnreadCountTutor = 1;

        await _db.SaveChangesAsync();

        return new ConversationDto(
            conv.Id, tutor.Id, tutor.Name, tutor.Role.ToString(),
            conv.LastMessagePreview, conv.LastMessageAt,
            0, conv.Status.ToString(),
            conv.RequestMessage
        );
    }

    public async Task MarkAsReadAsync(int conversationId, int userId)
    {
        var conv = await _db.Conversations.FindAsync(conversationId);
        if (conv == null) return;

        if (conv.StudentId != userId && conv.TutorId != userId) return;

        // Reset unread count for this user
        if (conv.StudentId == userId)
            conv.UnreadCountStudent = 0;
        else
            conv.UnreadCountTutor = 0;

        // Mark all messages from the other party as read
        var otherUserId = conv.StudentId == userId ? conv.TutorId : conv.StudentId;
        var unread = await _db.Messages
            .Where(m => m.ConversationId == conversationId
                     && m.SenderId == otherUserId
                     && m.ReadAt == null)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var msg in unread)
            msg.ReadAt = now;

        await _db.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        var asStudent = await _db.Conversations
            .Where(c => c.StudentId == userId)
            .SumAsync(c => c.UnreadCountStudent);

        var asTutor = await _db.Conversations
            .Where(c => c.TutorId == userId)
            .SumAsync(c => c.UnreadCountTutor);

        return asStudent + asTutor;
    }

    public async Task<string> GetUserNameAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        return user?.Name ?? "—";
    }

    public async Task<bool> ArchiveConversationAsync(int conversationId, int userId)
    {
        var conv = await _db.Conversations.FindAsync(conversationId);
        if (conv == null) return false;
        if (conv.StudentId != userId && conv.TutorId != userId) return false;
        if (conv.Status == ConversationStatus.Archived) return true;

        conv.Status = ConversationStatus.Archived;
        await _db.SaveChangesAsync();
        return true;
    }
}
