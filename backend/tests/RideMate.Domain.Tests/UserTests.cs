using RideMate.Domain.Entities;
using Xunit;

namespace RideMate.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Create_Under18_Throws()
    {
        var dob = DateTime.UtcNow.AddYears(-17);
        Assert.Throws<ArgumentException>(() => User.Create("a@b.pl", "hash", dob));
    }

    [Fact]
    public void Create_Exactly18_Succeeds()
    {
        var dob = DateTime.UtcNow.AddYears(-18);
        var user = User.Create("a@b.pl", "hash", dob);
        Assert.Equal("a@b.pl", user.Email);
        Assert.False(user.IsEmailConfirmed);
    }

    [Fact]
    public void ConfirmEmail_MarksConfirmed()
    {
        var user = User.Create("a@b.pl", "hash", DateTime.UtcNow.AddYears(-20));
        user.ConfirmEmail();
        Assert.True(user.IsEmailConfirmed);
        Assert.Null(user.EmailConfirmationToken);
    }
}
