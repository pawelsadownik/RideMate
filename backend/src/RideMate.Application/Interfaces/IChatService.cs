using RideMate.Application.Dto;

namespace RideMate.Application.Interfaces;

public interface IChatService
{
    Task<List<ConversationDto>> GetConversationsAsync(Guid userId, CancellationToken ct = default);
    Task<List<MessageDto>> GetMessagesAsync(Guid userId, Guid conversationId, int page = 1, int pageSize = 50, CancellationToken ct = default);
    Task<MessageDto> SendMessageAsync(Guid userId, Guid conversationId, SendMessageRequest request, CancellationToken ct = default);
    Task MarkConversationReadAsync(Guid userId, Guid conversationId, CancellationToken ct = default);
}
