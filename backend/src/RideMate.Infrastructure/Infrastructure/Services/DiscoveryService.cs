using Microsoft.EntityFrameworkCore;
using RideMate.Application.Dto;
using RideMate.Application.Interfaces;
using RideMate.Domain;
using RideMate.Domain.Entities;
using RideMate.Infrastructure.Persistence;

namespace RideMate.Infrastructure.Services;

public sealed class DiscoveryService : IDiscoveryService
{
    private readonly RideMateDbContext _db;
    private readonly IPushNotificationService _push;

    public DiscoveryService(RideMateDbContext db, IPushNotificationService push)
    {
        _db = db;
        _push = push;
    }

    public async Task<List<DiscoveryProfileDto>> GetDiscoveryProfilesAsync(Guid currentUserId, DiscoveryFilters filters, CancellationToken ct = default)
    {
        var current = await _db.Users
            .Include(u => u.Profile!).ThenInclude(p => p.Photos!)
            .FirstOrDefaultAsync(u => u.Id == currentUserId && !u.IsDeleted, ct)
            ?? throw new InvalidOperationException("Użytkownik nie został znaleziony.");

        if (current.Profile is null)
            return new List<DiscoveryProfileDto>();

        var blockedIds = await _db.Blocks
            .Where(b => b.BlockerUserId == currentUserId || b.BlockedUserId == currentUserId)
            .Select(b => b.BlockerUserId == currentUserId ? b.BlockedUserId : b.BlockerUserId)
            .ToListAsync(ct);

        var likedOrPassedIds = await _db.Likes
            .Where(l => l.FromUserId == currentUserId)
            .Select(l => l.ToUserId)
            .ToListAsync(ct);

        var candidates = await _db.Users
            .Include(u => u.Profile!).ThenInclude(p => p.Photos!)
            .Where(u => u.Id != currentUserId
                && !u.IsDeleted
                && !blockedIds.Contains(u.Id)
                && !likedOrPassedIds.Contains(u.Id))
            .ToListAsync(ct);

        var result = new List<DiscoveryProfileDto>();
        foreach (var u in candidates)
        {
            if (u.Profile is null || !u.Profile.IsActive) continue;
            var p = u.Profile!;
            if (!string.IsNullOrWhiteSpace(filters.ActivityType)
                && Enum.TryParse<ActivityType>(filters.ActivityType, true, out var activity)
                && p.ActivityType != activity && p.ActivityType != ActivityType.Both)
                continue;

            if (filters.MinAge.HasValue && p.Age < filters.MinAge.Value) continue;
            if (filters.MaxAge.HasValue && p.Age > filters.MaxAge.Value) continue;

            if (!string.IsNullOrWhiteSpace(filters.LookingFor)
                && Enum.TryParse<LookingFor>(filters.LookingFor, true, out var lookingFor)
                && p.LookingFor != lookingFor)
                continue;

            if (!string.IsNullOrWhiteSpace(filters.RidingStyle)
                && !string.IsNullOrWhiteSpace(p.RidingStyle)
                && p.RidingStyle.Contains(filters.RidingStyle, StringComparison.OrdinalIgnoreCase))
            {
                // pass - matches filter
            }
            else if (!string.IsNullOrWhiteSpace(filters.RidingStyle))
            {
                continue;
            }

            var distance = CalculateDistanceKm(current.Profile!.Latitude, current.Profile!.Longitude,
                                               p.Latitude, p.Longitude);
            if (filters.MaxDistanceKm.HasValue && distance > filters.MaxDistanceKm.Value)
                continue;

            result.Add(Map(u, distance));
        }

        return result.OrderBy(x => x.DistanceKm).ToList();
    }

    public async Task<LikeResult> LikeOrPassAsync(Guid fromUserId, LikeActionRequest request, CancellationToken ct = default)
    {
        var from = await _db.Users.FirstOrDefaultAsync(u => u.Id == fromUserId && !u.IsDeleted, ct)
            ?? throw new InvalidOperationException("Użytkownik nie został znaleziony.");
        var to = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.ToUserId && !u.IsDeleted, ct)
            ?? throw new InvalidOperationException("Użytkownik docelowy nie został znaleziony.");

        if (fromUserId == request.ToUserId)
            throw new InvalidOperationException("Nie można polubić ani pominąć samego siebie.");

        var existing = await _db.Likes
            .FirstOrDefaultAsync(l => l.FromUserId == fromUserId && l.ToUserId == request.ToUserId, ct);

        bool isMatch = false;
        Guid? matchId = null;

        using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            if (existing is not null)
            {
                existing.ChangeType(request.Like ? LikeType.Like : LikeType.Pass);
            }
            else
            {
                var like = Like.Create(fromUserId, request.ToUserId, request.Like ? LikeType.Like : LikeType.Pass);
                _db.Likes.Add(like);
            }

            if (request.Like)
            {
                var backLike = await _db.Likes
                    .FirstOrDefaultAsync(l => l.FromUserId == request.ToUserId && l.ToUserId == fromUserId && l.Type == LikeType.Like, ct);

                if (backLike is not null)
                {
                    var match = await _db.Matches
                        .FirstOrDefaultAsync(m => (m.User1Id == fromUserId && m.User2Id == request.ToUserId)
                                                || (m.User1Id == request.ToUserId && m.User2Id == fromUserId), ct);

                    if (match is null)
                    {
                        match = Match.Create(fromUserId, request.ToUserId);
                        _db.Matches.Add(match);
                        await _db.SaveChangesAsync(ct);

                        var conversation = Conversation.Create(match.Id);
                        _db.Conversations.Add(conversation);
                        await _db.SaveChangesAsync(ct);

                        matchId = match.Id;

                        // Powiadomienia push o nowym dopasowaniu (oba użytkownicy)
                        var fromUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == fromUserId, ct);
                        var toUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.ToUserId, ct);
                        _ = _push.SendMatchNotificationAsync(request.ToUserId, fromUser?.Profile?.Nickname ?? "Użytkownik", ct);
                        _ = _push.SendMatchNotificationAsync(fromUserId, toUser?.Profile?.Nickname ?? "Użytkownik", ct);
                    }
                    else
                    {
                        matchId = match.Id;
                    }

                    isMatch = match.IsActive;
                }
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        return new LikeResult(isMatch, matchId);
    }

    public async Task<bool> HasRemainingLikesTodayAsync(Guid userId, CancellationToken ct = default)
    {
        var remaining = await RemainingLikesTodayAsync(userId, ct);
        return remaining > 0;
    }

    public async Task<int> RemainingLikesTodayAsync(Guid userId, CancellationToken ct = default)
    {
        var subscription = await _db.SubscriptionEntitlements
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);
        var isPremium = subscription is not null && subscription.IsActive();

        var todayStart = DateTime.UtcNow.Date;
        var likesToday = await _db.Likes
            .CountAsync(l => l.FromUserId == userId && l.Type == LikeType.Like && l.CreatedAt >= todayStart, ct);

        int limit = isPremium ? 1000 : 50;
        return Math.Max(0, limit - likesToday);
    }

    private static double CalculateDistanceKm(double? lat1, double? lon1, double? lat2, double? lon2)
    {
        if (lat1 is null || lon1 is null || lat2 is null || lon2 is null) return double.MaxValue;
        const double R = 6371.0;
        var dLat = Deg2Rad(lat2.Value - lat1.Value);
        var dLon = Deg2Rad(lon2.Value - lon1.Value);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(Deg2Rad(lat1.Value)) * Math.Cos(Deg2Rad(lat2.Value)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double Deg2Rad(double deg) => deg * (Math.PI / 180);

    private static DiscoveryProfileDto Map(User u, double distance)
    {
        var p = u.Profile!;
        return new DiscoveryProfileDto(
            u.Id,
            p.Nickname,
            p.Age,
            Math.Round(distance, 1),
            p.ActivityType.ToString(),
            p.LookingFor.ToString(),
            p.RidingStyle,
            p.Bio,
            p.City,
            p.Photos.OrderBy(x => x.OrderIndex).FirstOrDefault()?.Url,
            p.Photos.OrderBy(x => x.OrderIndex).Select(x => x.Url).ToList()
        );
    }
}
