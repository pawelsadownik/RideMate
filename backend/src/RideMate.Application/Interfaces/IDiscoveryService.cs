using RideMate.Application.Dto;

namespace RideMate.Application.Interfaces;

public interface IDiscoveryService
{
    Task<List<DiscoveryProfileDto>> GetDiscoveryProfilesAsync(Guid currentUserId, DiscoveryFilters filters, CancellationToken ct = default);
    Task<LikeResult> LikeOrPassAsync(Guid fromUserId, LikeActionRequest request, CancellationToken ct = default);
    Task<bool> HasRemainingLikesTodayAsync(Guid userId, CancellationToken ct = default);
    Task<int> RemainingLikesTodayAsync(Guid userId, CancellationToken ct = default);
}
