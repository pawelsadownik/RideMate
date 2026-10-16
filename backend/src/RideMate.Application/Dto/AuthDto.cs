namespace RideMate.Application.Dto;

public sealed record RegisterRequest(string Email, string Password, DateTime DateOfBirth);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string Token, DateTime ExpiresAt, Guid UserId);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Token, string NewPassword);
public sealed record ChangeEmailRequest(string NewEmail);
