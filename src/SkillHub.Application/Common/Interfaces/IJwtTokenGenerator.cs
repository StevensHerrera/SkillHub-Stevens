using SkillHub.Application.Common.Models;

namespace SkillHub.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    AuthToken Generate(AuthUser user, Guid? participantId, Guid? instructorId);
}

public sealed record AuthToken(string AccessToken, DateTime ExpiresAt);
