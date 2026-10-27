using SkillHub.Application.Common.Interfaces;

namespace SkillHub.Infrastructure.Identity;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
