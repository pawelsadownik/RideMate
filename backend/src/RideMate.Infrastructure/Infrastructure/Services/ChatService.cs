using Microsoft.EntityFrameworkCore;
using RideMate.Application.Dto;
using RideMate.Application.Interfaces;
using RideMate.Domain.Entities;
using RideMate.Infrastructure.Persistence;

namespace RideMate.Infrastructure.Services;

public sealed class ChatService : IChatService
{
    private readonly RideMateDbContext _db;
    private readonly IPushNotificationService _push;

    public ChatService(RideMateDbContext db, IPushNotificationService push)
    {
        _db = db;
        _push = push;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(Guid userId, CancellationToken ct = default)
    {
        var conversations = await _db.Conversations
            .Include(c => c.Match!)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .Where(c => (c.Match!.User1Id == userId || c.Match!.User2Id == userId) && c.Match!.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        var result = new List<ConversationDto>();
        foreach (var c in conversations)
        {
            var match = c.Match!;
            var otherUserId = match.OtherUserId(userId);
            var other = await _db.Users.Include(u => u.Profile).ThenInclude(p => p.Photos)
                .FirstOrDefaultAsync(u => u.Id == otherUserId, ct);
            var otherProfile = other?.Profile;
            var primaryPhoto = otherProfile?.Photos.OrderBy(p => p.OrderIndex).FirstOrDefault()?.Url;
            var lastMessage = c.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            var unreadCount = await _db.Messages
                .CountAsync(m => m.ConversationId == c.Id && m.SenderUserId != userId && m.ReadAt == null, ct);

            result.Add(new ConversationDto(
                c.Id,
                c.MatchId,
                otherUserId,
                otherProfile?.Nickname ?? "Użytkownik",
                primaryPhoto,
                unreadCount,
                lastMessage?.CreatedAt ?? DateTime.MinValue,
                lastMessage?.Text
            ));
        }

        return result;
    }

    public async Task<List<MessageDto>> GetMessagesAsync(Guid userId, Guid conversationId, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        var conversation = await _db.Conversations
            .Include(c => c.Match)
            .FirstOrDefaultAsync(c => c.Id == conversationId && (c.Match.User1Id == userId || c.Match.User2Id == userId), ct)
            ?? throw new InvalidOperationException("Konwersacja nie została znaleziona.");

        var messages = await _db.Messages
            .Include(m => m.Sender).ThenInclude(u => u.Profile)
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

        return messages.Select(m => new MessageDto(
            m.Id,
            m.SenderUserId,
            m.Sender?.Profile?.Nickname ?? "Użytkownik",
            m.Text,
            m.CreatedAt,
            m.ReadAt is not null
        )).ToList();
    }

    public async Task<MessageDto> SendMessageAsync(Guid userId, Guid conversationId, SendMessageRequest request, CancellationToken ct = default)
    {
        var conversation = await _db.Conversations
            .Include(c => c.Match!)
            .FirstOrDefaultAsync(c => c.Id == conversationId && (c.Match!.User1Id == userId || c.Match!.User2Id == userId) && c.Match!.IsActive, ct)
            ?? throw new InvalidOperationException("Konwersacja nie została znaleziona.");

        var message = Message.Create(conversationId, userId, request.Text);
        _db.Messages.Add(message);
        await _db.SaveChangesAsync(ct);

        var sender = await _db.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == userId, ct);
        var senderNickname = sender?.Profile?.Nickname ?? "Ty";
        var otherUserId = conversation.Match!.OtherUserId(userId);

        // Powiadomienie push o nowej wiadomości (nie blokuje głównego przepływu)
        _ = _push.SendMessageNotificationAsync(otherUserId, senderNickname, request.Text, ct);

        return new MessageDto(
            message.Id,
            message.SenderUserId,
            senderNickname,
            message.Text,
            message.CreatedAt,
            message.ReadAt is not null
        );
    }

    public async Task MarkConversationReadAsync(Guid userId, Guid conversationId, CancellationToken ct = default)
    {
        var conversation = await _db.Conversations
            .Include(c => c.Match!)
            .FirstOrDefaultAsync(c => c.Id == conversationId && (c.Match!.User1Id == userId || c.Match!.User2Id == userId), ct)
            ?? throw new InvalidOperationException("Konwersacja nie została znaleziona.");

        var unread = await _db.Messages
            .Where(m => m.ConversationId == conversationId && m.SenderUserId != userId && m.ReadAt == null)
            .ToListAsync(ct);

        foreach (var m in unread) m.MarkRead();
        await _db.SaveChangesAsync(ct);
    }
}
