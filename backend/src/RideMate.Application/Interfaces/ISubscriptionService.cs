using RideMate.Application.Dto;

namespace RideMate.Application.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionDto> GetSubscriptionAsync(Guid userId, CancellationToken ct = default);
    Task ActivatePremiumAsync(Guid userId, string platform, string? productId, string? transactionId, DateTime? expiresAt, CancellationToken ct = default);
    Task ExpireSubscriptionAsync(Guid userId, CancellationToken ct = default);
    // Tryb developerski — wyłączony w produkcji
    Task SetDevPremiumAsync(Guid userId, bool isPremium, bool isProduction, CancellationToken ct = default);
    Task<bool> IsPremiumAsync(Guid userId, CancellationToken ct = default);
    Task<int> DailyLikeLimitAsync(Guid userId, CancellationToken ct = default);
}
