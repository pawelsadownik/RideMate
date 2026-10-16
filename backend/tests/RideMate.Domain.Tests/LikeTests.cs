using RideMate.Domain;
using RideMate.Domain.Entities;
using Xunit;

namespace RideMate.Domain.Tests;

public class LikeTests
{
    [Fact]
    public void Create_Like_SetsTypeLike()
    {
        var like = Like.Create(Guid.NewGuid(), Guid.NewGuid(), LikeType.Like);
        Assert.Equal(LikeType.Like, like.Type);
    }

    [Fact]
    public void Create_Pass_SetsTypePass()
    {
        var like = Like.Create(Guid.NewGuid(), Guid.NewGuid(), LikeType.Pass);
        Assert.Equal(LikeType.Pass, like.Type);
    }

    [Fact]
    public void Create_SameUser_Throws()
    {
        var id = Guid.NewGuid();
        Assert.Throws<InvalidOperationException>(() => Like.Create(id, id, LikeType.Like));
    }

    [Fact]
    public void ChangeType_UpdatesType()
    {
        var like = Like.Create(Guid.NewGuid(), Guid.NewGuid(), LikeType.Pass);
        like.ChangeType(LikeType.Like);
        Assert.Equal(LikeType.Like, like.Type);
    }
}
