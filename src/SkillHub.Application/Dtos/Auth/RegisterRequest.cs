using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.Dtos.Auth;

public sealed record RegisterRequest(
    [Required, StringLength(200, MinimumLength = 3)] string FullName,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(100, MinimumLength = 10)] string Password);
