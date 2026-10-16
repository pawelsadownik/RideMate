using RideMate.Application.Dto;

namespace RideMate.Application.Interfaces;

public interface IMatchService
{
    Task<List<MatchDto>> GetMatchesAsync(Guid userId, CancellationToken ct = default);
    Task<bool> UnmatchAsync(Guid userId, Guid matchId, CancellationToken ct = default);
}
