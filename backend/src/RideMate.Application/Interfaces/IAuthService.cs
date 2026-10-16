using RideMate.Application.Dto;

namespace RideMate.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(string email, string password, DateTime dateOfBirth, string? ipAddress = null, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(string email, string password, string? ipAddress = null, CancellationToken ct = default);
    Task ConfirmEmailAsync(string token, CancellationToken ct = default);
    Task ForgotPasswordAsync(string email, CancellationToken ct = default);
    Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default);
    Task DeleteAccountAsync(Guid userId, CancellationToken ct = default);
}
