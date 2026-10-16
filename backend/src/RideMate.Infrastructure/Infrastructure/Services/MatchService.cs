using Microsoft.EntityFrameworkCore;
using RideMate.Application.Dto;
using RideMate.Application.Interfaces;
using RideMate.Infrastructure.Persistence;

namespace RideMate.Infrastructure.Services;

public sealed class MatchService : IMatchService
{
    private readonly RideMateDbContext _db;

    public MatchService(RideMateDbContext db) => _db = db;

    public async Task<List<MatchDto>> GetMatchesAsync(Guid userId, CancellationToken ct = default)
    {
        var matches = await _db.Matches
            .Include(m => m.User1!).ThenInclude(u => u.Profile!).ThenInclude(p => p.Photos!)
            .Include(m => m.User2!).ThenInclude(u => u.Profile!).ThenInclude(p => p.Photos!)
            .Where(m => (m.User1Id == userId || m.User2Id == userId) && m.IsActive)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        var result = new List<MatchDto>();
        foreach (var m in matches)
        {
            var other = m.User1Id == userId ? m.User2 : m.User1;
            if (other is null) continue;
            var otherProfile = other.Profile;
            var primaryPhoto = otherProfile?.Photos.OrderBy(p => p.OrderIndex).FirstOrDefault()?.Url;

            result.Add(new MatchDto(
                m.Id,
                other.Id,
                otherProfile?.Nickname ?? "Użytkownik",
                otherProfile?.Age ?? 0,
                primaryPhoto,
                m.CreatedAt,
                m.IsActive
            ));
        }

        return result;
    }

    public async Task<bool> UnmatchAsync(Guid userId, Guid matchId, CancellationToken ct = default)
    {
        var match = await _db.Matches
            .FirstOrDefaultAsync(m => m.Id == matchId && (m.User1Id == userId || m.User2Id == userId) && m.IsActive, ct)
            ?? throw new InvalidOperationException("Dopasowanie nie zostało znalezione.");

        match.Unmatch();
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
