using Microsoft.EntityFrameworkCore;
using RideMate.Application.Interfaces;
using RideMate.Domain.Entities;
using RideMate.Infrastructure.Persistence;

namespace RideMate.Infrastructure.Services;

public sealed class PushTokenService : IPushTokenService
{
    private readonly RideMateDbContext _db;

    public PushTokenService(RideMateDbContext db) => _db = db;

    public async Task UpsertTokenAsync(Guid userId, string token, string platform, CancellationToken ct = default)
    {
        var existing = await _db.PushTokens.FirstOrDefaultAsync(p => p.Token == token, ct);
        if (existing is not null)
        {
            existing.Reassign(userId, platform);
        }
        else
        {
            _db.PushTokens.Add(PushToken.Create(userId, token, platform));
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<string>> GetTokensForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.PushTokens
            .Where(p => p.UserId == userId)
            .Select(p => p.Token)
            .ToListAsync(ct);
    }
}
