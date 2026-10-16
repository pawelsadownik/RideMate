namespace RideMate.Application.Dto;

public sealed record SubscriptionDto(bool IsPremium, DateTime? ExpiresAt, string? Platform);
public sealed record GrantPremiumRequest(Guid UserId, int Months);
public sealed record DevTogglePremiumRequest(Guid UserId, bool IsPremium, bool IsProduction);
