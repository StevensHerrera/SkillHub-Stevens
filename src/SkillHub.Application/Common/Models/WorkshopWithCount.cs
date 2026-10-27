using SkillHub.Domain.Entities;

namespace SkillHub.Application.Common.Models;

public sealed record WorkshopWithCount(Workshop Workshop, int ActiveEnrollments);
