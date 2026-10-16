using Microsoft.EntityFrameworkCore;
using RideMate.Application.Dto;
using RideMate.Application.Interfaces;
using RideMate.Domain;
using RideMate.Domain.Entities;
using RideMate.Infrastructure.Persistence;

namespace RideMate.Infrastructure.Services;

public sealed class ProfileService : IProfileService
{
    private readonly RideMateDbContext _db;

    public ProfileService(RideMateDbContext db) => _db = db;

    public async Task<ProfileDto?> GetMyProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _db.Profiles
            .Include(p => p.Photos)
            .Include(p => p.RidingPreferences)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        return profile is null ? null : Map(profile);
    }

    public async Task<ProfileDto> GetProfileByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _db.Profiles
            .Include(p => p.Photos)
            .Include(p => p.RidingPreferences)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive, ct)
            ?? throw new InvalidOperationException("Profil nie został znaleziony.");

        return Map(profile);
    }

    public async Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var profile = await _db.Profiles
            .Include(p => p.Photos)
            .Include(p => p.RidingPreferences)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new InvalidOperationException("Profil nie został znaleziony.");

        if (!Enum.TryParse<LookingFor>(request.LookingFor, true, out var lookingFor))
            throw new ArgumentException("Nieprawidłowa wartość 'lookingFor'.", nameof(request.LookingFor));
        if (!Enum.TryParse<ActivityType>(request.ActivityType, true, out var activityType))
            throw new ArgumentException("Nieprawidłowa wartość 'activityType'.", nameof(request.ActivityType));

        profile.Update(
            request.Nickname,
            request.Age,
            lookingFor,
            activityType,
            request.RidingStyle,
            request.Bio,
            request.City,
            request.Latitude,
            request.Longitude);

        await _db.SaveChangesAsync(ct);
        return Map(profile);
    }

    public async Task<RidingPreferencesDto> UpdateRidingPreferencesAsync(Guid userId, UpdateRidingPreferencesRequest request, CancellationToken ct = default)
    {
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new InvalidOperationException("Profil nie został znaleziony.");

        var prefs = profile.RidingPreferences ?? RidingPreferences.Create(profile.Id);
        if (profile.RidingPreferences is null)
        {
            _db.RidingPreferences.Add(prefs);
            profile.RidingPreferences = prefs;
        }

        prefs.UpdateMotorcycle(
            request.MotorcycleType,
            request.MotorcycleBrand,
            request.MotorcycleModel,
            request.RidesOwnMotorcycle ?? prefs.RidesOwnMotorcycle,
            request.CanTakePassenger ?? prefs.CanTakePassenger,
            request.WantsToBePassenger ?? prefs.WantsToBePassenger);

        if (profile.RidingPreferences is null)
            profile.RidingPreferences = prefs;

        prefs.UpdateBicycles(
            request.RoadBike ?? prefs.RoadBike,
            request.Gravel ?? prefs.Gravel,
            request.Mtb ?? prefs.Mtb,
            request.Trekking ?? prefs.Trekking,
            request.CityBike ?? prefs.CityBike,
            request.Recreational ?? prefs.Recreational);

        await _db.SaveChangesAsync(ct);
        return MapPreferences(prefs);
    }

    public async Task AddPhotoAsync(Guid userId, AddPhotoRequest request, CancellationToken ct = default)
    {
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new InvalidOperationException("Profil nie został znaleziony.");

        var photoCount = await _db.ProfilePhotos.CountAsync(p => p.ProfileId == profile.Id, ct);
        if (photoCount >= 5)
            throw new InvalidOperationException("Maksymalna liczba zdjęć to 5.");

        var isPrimary = request.IsPrimary || photoCount == 0;
        if (request.IsPrimary)
        {
            foreach (var p in profile.Photos) p.SetAsPrimary();
        }

        var photo = ProfilePhoto.Create(profile.Id, request.Url, request.StoragePath, isPrimary, photoCount);
        _db.ProfilePhotos.Add(photo);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemovePhotoAsync(Guid userId, Guid photoId, CancellationToken ct = default)
    {
        var photo = await _db.ProfilePhotos
            .Include(p => p.Profile)
            .FirstOrDefaultAsync(p => p.Id == photoId && p.Profile!.UserId == userId, ct)
            ?? throw new InvalidOperationException("Zdjęcie nie zostało znalezione.");

        var profileId = photo.ProfileId;
        var wasPrimary = photo.IsPrimary;
        _db.ProfilePhotos.Remove(photo);
        await _db.SaveChangesAsync(ct);

        if (wasPrimary)
        {
            var next = await _db.ProfilePhotos
                .Where(p => p.ProfileId == profileId)
                .OrderBy(p => p.OrderIndex)
                .FirstOrDefaultAsync(ct);
            if (next is not null) next.SetAsPrimary();
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task SetPrimaryPhotoAsync(Guid userId, Guid photoId, CancellationToken ct = default)
    {
        var photo = await _db.ProfilePhotos
            .Include(p => p.Profile)
            .FirstOrDefaultAsync(p => p.Id == photoId && p.Profile!.UserId == userId, ct)
            ?? throw new InvalidOperationException("Zdjęcie nie zostało znalezione.");

        var profile = photo.Profile!;
        foreach (var p in profile.Photos) p.SetAsPrimary();
        photo.SetAsPrimary();
        await _db.SaveChangesAsync(ct);
    }

    private static ProfileDto Map(Profile p) => new(
        p.Id, p.UserId, p.Nickname, p.Age, p.Bio, p.City, p.Latitude, p.Longitude,
        p.LookingFor.ToString(), p.ActivityType.ToString(), p.RidingStyle,
        p.Photos.OrderBy(x => x.OrderIndex).Select(ph => new ProfilePhotoDto(ph.Id, ph.Url, ph.IsPrimary, ph.OrderIndex)).ToList(),
        p.RidingPreferences is null ? null : MapPreferences(p.RidingPreferences),
        p.IsActive
    );

    private static RidingPreferencesDto MapPreferences(RidingPreferences p) => new(
        p.MotorcycleType, p.MotorcycleBrand, p.MotorcycleModel,
        p.RidesOwnMotorcycle, p.CanTakePassenger, p.WantsToBePassenger,
        p.RoadBike, p.Gravel, p.Mtb, p.Trekking, p.CityBike, p.Recreational
    );
}
