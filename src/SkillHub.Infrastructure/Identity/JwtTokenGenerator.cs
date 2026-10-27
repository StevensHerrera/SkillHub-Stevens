using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.Common.Models;

namespace SkillHub.Infrastructure.Identity;

public sealed class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
    public AuthToken Generate(AuthUser user, Guid? participantId, Guid? instructorId)
    {
        var keyValue = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        if (Encoding.UTF8.GetByteCount(keyValue) < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 bytes long.");

        var issuer = configuration["Jwt:Issuer"] ?? "SkillHub.Api";
        var audience = configuration["Jwt:Audience"] ?? "SkillHub.Web";
        var expiresAt = DateTime.UtcNow.AddMinutes(configuration.GetValue("Jwt:ExpiresMinutes", 120));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName)
        };

        if (participantId is Guid participant) claims.Add(new Claim("participant_id", participant.ToString()));
        if (instructorId is Guid instructor) claims.Add(new Claim("instructor_id", instructor.ToString()));
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            DateTime.UtcNow,
            expiresAt,
            new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new AuthToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
