using RideMate.Domain;
using RideMate.Domain.Entities;
using Xunit;

namespace RideMate.Domain.Tests;

public class SubscriptionTests
{
    [Fact]
    public void ActivatePremium_SetsIsPremiumTrue()
    {
        var sub = SubscriptionEntitlement.Create(Guid.NewGuid());
        sub.ActivatePremium(SubscriptionPlatform.Apple, "prod_1", "txn_1", DateTime.UtcNow.AddDays(30));
        Assert.True(sub.IsActive());
        Assert.Equal(SubscriptionPlatform.Apple, sub.Platform);
    }

    [Fact]
    public void IsActive_FalseWhenExpired()
    {
        var sub = SubscriptionEntitlement.Create(Guid.NewGuid());
        sub.ActivatePremium(SubscriptionPlatform.Google, "prod_1", "txn_1", DateTime.UtcNow.AddDays(-1));
        Assert.False(sub.IsActive());
    }

    [Fact]
    public void IsActive_FalseWhenNotPremium()
    {
        var sub = SubscriptionEntitlement.Create(Guid.NewGuid());
        Assert.False(sub.IsActive());
    }

    [Fact]
    public void Expire_SetsIsPremiumFalse()
    {
        var sub = SubscriptionEntitlement.Create(Guid.NewGuid());
        sub.ActivatePremium(SubscriptionPlatform.Apple, "p", "t", DateTime.UtcNow.AddDays(30));
        sub.Expire();
        Assert.False(sub.IsPremium);
    }
}
