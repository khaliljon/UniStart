using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IMessageService
{
    Task<List<ConversationDto>> GetConversationsAsync(int userId);
    Task<MessagesPageDto> GetMessagesAsync(int conversationId, int userId, int page, int pageSize);
    Task<MessageDto> SendMessageAsync(int senderId, int conversationId, string text);
    Task<ConversationDto> StartConversationAsync(int studentId, int tutorId);
    Task MarkAsReadAsync(int conversationId, int userId);
    Task<int> GetUnreadCountAsync(int userId);
}
