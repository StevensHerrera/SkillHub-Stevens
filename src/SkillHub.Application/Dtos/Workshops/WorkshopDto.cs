namespace SkillHub.Application.Dtos.Workshops;

public sealed record WorkshopDto(
    Guid Id,
    string Code,
    string Title,
    string Description,
    string InstructorName,
    string InstructorEmail,
    string InstructorSpecialty,
    DateTime StartAt,
    DateTime EndAt,
    int Capacity,
    int ActiveEnrollments,
    int AvailableSeats);
