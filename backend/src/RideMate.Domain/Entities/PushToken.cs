using RideMate.Domain.Common;

namespace RideMate.Domain.Entities;

public sealed class PushToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public string Platform { get; private set; } = string.Empty; // ios / android
    public DateTime LastUsedAt { get; private set; } = DateTime.UtcNow;

    public User? User { get; private set; }

    private PushToken() { }

    public static PushToken Create(Guid userId, string token, string platform)
    {
        return new PushToken
        {
            UserId = userId,
            Token = token,
            Platform = platform
        };
    }

    public void Reassign(Guid userId, string platform)
    {
        UserId = userId;
        Platform = platform;
        TouchUsed();
    }

    public void TouchUsed() => LastUsedAt = DateTime.UtcNow;
}
