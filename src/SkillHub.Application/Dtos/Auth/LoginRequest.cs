using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.Dtos.Auth;

public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(100, MinimumLength = 1)] string Password);
