using RideMate.Domain.Common;

namespace RideMate.Domain.Entities;

public sealed class Match : BaseEntity
{
    public Guid User1Id { get; private set; } // mniejszy Id, dla unikalności pary
    public Guid User2Id { get; private set; } // większy Id
    public bool IsActive { get; private set; } = true; // false = usunięto dopasowanie (unmatch)

    public User? User1 { get; private set; }
    public User? User2 { get; private set; }
    public Conversation? Conversation { get; private set; }

    private Match() { }

    public static Match Create(Guid userAId, Guid userBId)
    {
        var (user1, user2) = userAId < userBId ? (userAId, userBId) : (userBId, userAId);
        return new Match { User1Id = user1, User2Id = user2, IsActive = true };
    }

    public Guid OtherUserId(Guid currentUserId)
    {
        return currentUserId == User1Id ? User2Id : User1Id;
    }

    public void Unmatch() { IsActive = false; Touch(); }
    public void Reactivate() { IsActive = true; Touch(); }
}
