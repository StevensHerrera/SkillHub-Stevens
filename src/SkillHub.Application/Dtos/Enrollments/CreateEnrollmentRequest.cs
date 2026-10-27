using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.Dtos.Enrollments;

public sealed record CreateEnrollmentRequest([Required] Guid WorkshopId);
