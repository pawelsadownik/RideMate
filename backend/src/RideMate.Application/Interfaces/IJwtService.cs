namespace RideMate.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, DateTime expiresAt);
    Guid? ValidateToken(string token);
    int GetExpiryMinutes();
}
