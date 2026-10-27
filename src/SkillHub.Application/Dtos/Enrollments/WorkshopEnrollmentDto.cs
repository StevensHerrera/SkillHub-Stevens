namespace SkillHub.Application.Dtos.Enrollments;

public sealed record WorkshopEnrollmentDto(
    Guid Id,
    Guid ParticipantId,
    string ParticipantName,
    string ParticipantEmail,
    string Status,
    DateTime EnrolledAt,
    DateTime? CancelledAt,
    string? CancellationReason);
