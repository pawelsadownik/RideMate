namespace RideMate.Application.Dto;

public sealed record DiscoveryFilters(
    string? ActivityType,
    double? MaxDistanceKm,
    int? MinAge,
    int? MaxAge,
    string? LookingFor,
    string? RidingStyle
);

public sealed record DiscoveryProfileDto(
    Guid Id,
    string Nickname,
    int Age,
    double DistanceKm,
    string ActivityType,
    string LookingFor,
    string? RidingStyle,
    string? Bio,
    string? City,
    string? PrimaryPhotoUrl,
    List<string> PhotoUrls
);

public sealed record LikeActionRequest(Guid ToUserId, bool Like);
public sealed record LikeResult(bool IsMatch, Guid? MatchId);
