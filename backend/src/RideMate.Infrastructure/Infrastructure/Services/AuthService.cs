using Microsoft.EntityFrameworkCore;
using RideMate.Application.Dto;
using RideMate.Application.Interfaces;
using RideMate.Domain;
using RideMate.Domain.Entities;
using RideMate.Infrastructure.Persistence;

namespace RideMate.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly RideMateDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(RideMateDbContext db, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> RegisterAsync(string email, string password, DateTime dateOfBirth, string? ipAddress = null, CancellationToken ct = default)
    {
        if (password.Length < 8)
            throw new ArgumentException("Hasło musi mieć co najmniej 8 znaków.", nameof(password));

        var normalized = email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == normalized, ct))
            throw new InvalidOperationException("Użytkownik z tym adresem e-mail już istnieje.");

        var dobUtc = DateTime.SpecifyKind(dateOfBirth.Date, DateTimeKind.Utc);
        var age = DateTime.UtcNow.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.UtcNow.AddYears(-age)) age--;

        var nicknamePart = normalized.Split('@')[0];
        var nickname = nicknamePart.Length >= 2 ? nicknamePart[..Math.Min(nicknamePart.Length, 30)] : "user";

        var user = User.Create(normalized, _passwordHasher.Hash(password), dobUtc);
        _db.Users.Add(user);

        var profile = Profile.Create(user.Id, nickname, age, LookingFor.JointRides, ActivityType.Both, null, null, null, null, null);
        _db.Profiles.Add(profile);

        var subscription = SubscriptionEntitlement.Create(user.Id);
        _db.SubscriptionEntitlements.Add(subscription);

        await _db.SaveChangesAsync(ct);

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtService.GetExpiryMinutes());
        var token = _jwtService.GenerateToken(user.Id, user.Email, expiresAt);
        return new AuthResponse(token, expiresAt, user.Id);
    }

    public async Task<AuthResponse> LoginAsync(string email, string password, string? ipAddress = null, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var user = await _db.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == normalized && !u.IsDeleted, ct)
            ?? throw new InvalidOperationException("Nieprawidłowy adres e-mail lub hasło.");

        if (!_passwordHasher.Verify(password, user.PasswordHash))
            throw new InvalidOperationException("Nieprawidłowy adres e-mail lub hasło.");

        if (!user.IsEmailConfirmed)
            throw new InvalidOperationException("Adres e-mail nie został potwierdzony.");

        user.UpdateLastActive();
        await _db.SaveChangesAsync(ct);

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtService.GetExpiryMinutes());
        var token = _jwtService.GenerateToken(user.Id, user.Email, expiresAt);
        return new AuthResponse(token, expiresAt, user.Id);
    }

    public Task ConfirmEmailAsync(string token, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAccountAsync(Guid userId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
