namespace RideMate.Application.Dto;

public sealed record MatchDto(
    Guid Id,
    Guid OtherUserId,
    string OtherNickname,
    int OtherAge,
    string? OtherPrimaryPhotoUrl,
    DateTime? CreatedAt,
    bool IsActive
);

public sealed record ConversationDto(
    Guid Id,
    Guid MatchId,
    Guid OtherUserId,
    string OtherNickname,
    string? OtherPrimaryPhotoUrl,
    int UnreadCount,
    DateTime? LastMessageAt,
    string? LastMessageText
);

public sealed record MessageDto(
    Guid Id,
    Guid SenderUserId,
    string SenderNickname,
    string Text,
    DateTime SentAt,
    bool IsRead
);

public sealed record SendMessageRequest(string Text);
