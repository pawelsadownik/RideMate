using Microsoft.EntityFrameworkCore;
using RideMate.Application.Dto;
using RideMate.Application.Interfaces;
using RideMate.Domain;
using RideMate.Domain.Entities;
using RideMate.Infrastructure.Persistence;

namespace RideMate.Infrastructure.Services;

public sealed class SubscriptionService : ISubscriptionService
{
    private readonly RideMateDbContext _db;

    public SubscriptionService(RideMateDbContext db) => _db = db;

    public async Task<SubscriptionDto> GetSubscriptionAsync(Guid userId, CancellationToken ct = default)
    {
        var sub = await _db.SubscriptionEntitlements.FirstOrDefaultAsync(s => s.UserId == userId, ct);
        return new SubscriptionDto(sub is not null && sub.IsActive(), sub?.ExpiresAt, sub?.Platform.ToString());
    }

    public async Task ActivatePremiumAsync(Guid userId, string platform, string? productId, string? transactionId, DateTime? expiresAt, CancellationToken ct = default)
    {
        var sub = await _db.SubscriptionEntitlements.FirstOrDefaultAsync(s => s.UserId == userId, ct)
            ?? SubscriptionEntitlement.Create(userId);

        if (_db.SubscriptionEntitlements.Local.All(s => s.UserId != userId) && sub.Id == Guid.Empty)
            _db.SubscriptionEntitlements.Add(sub);

        sub.ActivatePremium(ParsePlatform(platform), productId, transactionId, expiresAt);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ExpireSubscriptionAsync(Guid userId, CancellationToken ct = default)
    {
        var sub = await _db.SubscriptionEntitlements.FirstOrDefaultAsync(s => s.UserId == userId, ct)
            ?? throw new InvalidOperationException("Subskrypcja nie została znaleziona.");

        sub.Expire();
        await _db.SaveChangesAsync(ct);
    }

    public async Task SetDevPremiumAsync(Guid userId, bool isPremium, bool isProduction, CancellationToken ct = default)
    {
        if (isProduction)
            throw new InvalidOperationException("Tryb deweloperski jest niedostępny w produkcji.");

        var sub = await _db.SubscriptionEntitlements.FirstOrDefaultAsync(s => s.UserId == userId, ct)
            ?? SubscriptionEntitlement.Create(userId);

        if (_db.SubscriptionEntitlements.Local.All(s => s.UserId != userId) && sub.Id == Guid.Empty)
            _db.SubscriptionEntitlements.Add(sub);

        if (isPremium)
            sub.ActivatePremium(SubscriptionPlatform.Development, null, null, null);
        else
            sub.Expire();

        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> IsPremiumAsync(Guid userId, CancellationToken ct = default)
    {
        var sub = await _db.SubscriptionEntitlements.FirstOrDefaultAsync(s => s.UserId == userId, ct);
        return sub is not null && sub.IsActive();
    }

    public async Task<int> DailyLikeLimitAsync(Guid userId, CancellationToken ct = default)
    {
        var isPremium = await IsPremiumAsync(userId, ct);
        return isPremium ? 1000 : 50;
    }

    private static SubscriptionPlatform ParsePlatform(string platform) => platform.ToLowerInvariant() switch
    {
        "apple" => SubscriptionPlatform.Apple,
        "google" => SubscriptionPlatform.Google,
        _ => SubscriptionPlatform.None
    };
}
