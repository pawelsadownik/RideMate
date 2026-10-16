using RideMate.Domain.Common;

namespace RideMate.Domain.Entities;

public sealed class SubscriptionEntitlement : BaseEntity
{
    public Guid UserId { get; private set; }
    public bool IsPremium { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public SubscriptionPlatform Platform { get; private set; } = SubscriptionPlatform.None;
    public string? ProductId { get; private set; }
    public string? TransactionId { get; private set; }

    public User? User { get; private set; }

    private SubscriptionEntitlement() { }

    public static SubscriptionEntitlement Create(Guid userId) => new() { UserId = userId, IsPremium = false };

    public void ActivatePremium(SubscriptionPlatform platform, string? productId, string? transactionId, DateTime? expiresAt)
    {
        IsPremium = true;
        Platform = platform;
        ProductId = productId;
        TransactionId = transactionId;
        ExpiresAt = expiresAt;
        Touch();
    }

    public void Expire()
    {
        IsPremium = false;
        ExpiresAt = null;
        Touch();
    }

    public bool IsActive() => IsPremium && (ExpiresAt is null || ExpiresAt > DateTime.UtcNow);
}
