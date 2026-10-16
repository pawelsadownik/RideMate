namespace RideMate.Application.Interfaces;

public interface IPushTokenService
{
    Task UpsertTokenAsync(Guid userId, string token, string platform, CancellationToken ct = default);
    Task<List<string>> GetTokensForUserAsync(Guid userId, CancellationToken ct = default);
}
