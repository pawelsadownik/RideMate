using RideMate.Domain.Common;

namespace RideMate.Domain.Entities;

public sealed class Block : BaseEntity
{
    public Guid BlockerUserId { get; private set; }
    public Guid BlockedUserId { get; private set; }

    public User? Blocker { get; private set; }
    public User? Blocked { get; private set; }

    private Block() { }

    public static Block Create(Guid blockerUserId, Guid blockedUserId)
    {
        if (blockerUserId == blockedUserId)
            throw new InvalidOperationException("Nie można zablokować samego siebie.");

        return new Block { BlockerUserId = blockerUserId, BlockedUserId = blockedUserId };
    }
}
