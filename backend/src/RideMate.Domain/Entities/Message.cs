using RideMate.Domain.Common;

namespace RideMate.Domain.Entities;

public sealed class Message : BaseEntity
{
    public Guid ConversationId { get; private set; }
    public Guid SenderUserId { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public DateTime? ReadAt { get; private set; }

    public Conversation? Conversation { get; private set; }
    public User? Sender { get; private set; }

    private Message() { }

    public static Message Create(Guid conversationId, Guid senderUserId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Wiadomość nie może być pusta.", nameof(text));
        if (text.Length > 1000)
            throw new ArgumentException("Wiadomość jest zbyt długa (maks. 1000 znaków).", nameof(text));

        return new Message
        {
            ConversationId = conversationId,
            SenderUserId = senderUserId,
            Text = text.Trim()
        };
    }

    public void MarkRead() => ReadAt = DateTime.UtcNow;
}
