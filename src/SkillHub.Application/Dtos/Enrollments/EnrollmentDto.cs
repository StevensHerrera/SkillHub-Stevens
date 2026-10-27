namespace SkillHub.Application.Dtos.Enrollments;

public sealed record EnrollmentDto(
    Guid Id,
    Guid WorkshopId,
    string WorkshopCode,
    string WorkshopTitle,
    string InstructorName,
    DateTime StartAt,
    DateTime EndAt,
    string Status,
    DateTime EnrolledAt,
    DateTime? CancelledAt,
    string? CancellationReason);
