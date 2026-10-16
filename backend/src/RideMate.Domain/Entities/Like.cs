using RideMate.Domain.Common;

namespace RideMate.Domain.Entities;

public sealed class Like : BaseEntity
{
    public Guid FromUserId { get; private set; }
    public Guid ToUserId { get; private set; }
    public LikeType Type { get; private set; }

    public User? FromUser { get; private set; }
    public User? ToUser { get; private set; }

    private Like() { }

    public static Like Create(Guid fromUserId, Guid toUserId, LikeType type)
    {
        if (fromUserId == toUserId)
            throw new InvalidOperationException("Nie można polubić ani pominąć własnego profilu.");

        return new Like
        {
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Type = type
        };
    }

    public void ChangeType(LikeType type) { Type = type; Touch(); }
}
