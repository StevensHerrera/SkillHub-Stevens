using System.Security.Claims;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Domain.Common;

namespace SkillHub.Api.Services;

public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public bool IsEmployee => User?.IsInRole(Roles.Employee) == true;

    public bool IsParticipant => User?.IsInRole(Roles.Participant) == true;
}
