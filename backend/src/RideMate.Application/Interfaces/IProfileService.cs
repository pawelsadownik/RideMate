using RideMate.Application.Dto;

namespace RideMate.Application.Interfaces;

public interface IProfileService
{
    Task<ProfileDto?> GetMyProfileAsync(Guid userId, CancellationToken ct = default);
    Task<ProfileDto> GetProfileByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<RidingPreferencesDto> UpdateRidingPreferencesAsync(Guid userId, UpdateRidingPreferencesRequest request, CancellationToken ct = default);
    Task AddPhotoAsync(Guid userId, AddPhotoRequest request, CancellationToken ct = default);
    Task RemovePhotoAsync(Guid userId, Guid photoId, CancellationToken ct = default);
    Task SetPrimaryPhotoAsync(Guid userId, Guid photoId, CancellationToken ct = default);
}
