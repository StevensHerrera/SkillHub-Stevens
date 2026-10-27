namespace SkillHub.Application.Dtos.Auth;

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string Email,
    string FullName,
    string[] Roles);
