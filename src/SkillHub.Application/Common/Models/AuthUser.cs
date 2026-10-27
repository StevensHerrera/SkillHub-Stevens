namespace SkillHub.Application.Common.Models;

public sealed record AuthUser(Guid Id, string Email, string FullName, IReadOnlyList<string> Roles);
