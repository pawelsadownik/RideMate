namespace RideMate.Application.Dto;

public sealed record ProfileDto(
    Guid Id,
    Guid UserId,
    string Nickname,
    int Age,
    string? Bio,
    string? City,
    double? Latitude,
    double? Longitude,
    string LookingFor,
    string ActivityType,
    string? RidingStyle,
    List<ProfilePhotoDto> Photos,
    RidingPreferencesDto? RidingPreferences,
    bool IsActive
);

public sealed record ProfilePhotoDto(Guid Id, string Url, bool IsPrimary, int OrderIndex);

public sealed record RidingPreferencesDto(
    string? MotorcycleType,
    string? MotorcycleBrand,
    string? MotorcycleModel,
    bool RidesOwnMotorcycle,
    bool CanTakePassenger,
    bool WantsToBePassenger,
    bool RoadBike,
    bool Gravel,
    bool Mtb,
    bool Trekking,
    bool CityBike,
    bool Recreational
);

public sealed record UpdateProfileRequest(
    string Nickname,
    int Age,
    string LookingFor,
    string ActivityType,
    string? RidingStyle,
    string? Bio,
    string? City,
    double? Latitude,
    double? Longitude
);

public sealed record UpdateRidingPreferencesRequest(
    string? MotorcycleType,
    string? MotorcycleBrand,
    string? MotorcycleModel,
    bool? RidesOwnMotorcycle,
    bool? CanTakePassenger,
    bool? WantsToBePassenger,
    bool? RoadBike,
    bool? Gravel,
    bool? Mtb,
    bool? Trekking,
    bool? CityBike,
    bool? Recreational
);

public sealed record AddPhotoRequest(string Url, string StoragePath, bool IsPrimary);
