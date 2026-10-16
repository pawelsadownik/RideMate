using RideMate.Domain.Common;

namespace RideMate.Domain.Entities;

public sealed class Conversation : BaseEntity
{
    public Guid MatchId { get; private set; }

    public Match? Match { get; private set; }
    public ICollection<Message> Messages { get; private set; } = new List<Message>();

    private Conversation() { }

    public static Conversation Create(Guid matchId) => new() { MatchId = matchId };
}
