namespace SkillHub.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsEmployee { get; }
    bool IsParticipant { get; }
}
