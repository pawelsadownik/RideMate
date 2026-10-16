using RideMate.Domain;
using RideMate.Domain.Entities;
using Xunit;

namespace RideMate.Domain.Tests;

public class MatchTests
{
    [Fact]
    public void Create_OrdersUsersById()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var match = Match.Create(a, b);

        if (a < b)
        {
            Assert.Equal(a, match.User1Id);
            Assert.Equal(b, match.User2Id);
        }
        else
        {
            Assert.Equal(b, match.User1Id);
            Assert.Equal(a, match.User2Id);
        }

        Assert.True(match.IsActive);
    }

    [Fact]
    public void OtherUserId_ReturnsCorrectCounterpart()
    {
        var a = Guid.NewGuid();
        var b = Guid.NewGuid();
        var match = Match.Create(a, b);
        Assert.Equal(b, match.OtherUserId(a));
        Assert.Equal(a, match.OtherUserId(b));
    }

    [Fact]
    public void Unmatch_SetsInactive()
    {
        var match = Match.Create(Guid.NewGuid(), Guid.NewGuid());
        match.Unmatch();
        Assert.False(match.IsActive);
        match.Reactivate();
        Assert.True(match.IsActive);
    }
}
