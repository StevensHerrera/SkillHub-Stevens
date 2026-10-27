using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.Dtos.Enrollments;

public sealed record CancelEnrollmentRequest([StringLength(500)] string? Reason);
