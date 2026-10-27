namespace SkillHub.Application.Dtos.Reports;

public sealed record TopWorkshopDto(
    Guid Id,
    string Code,
    string Title,
    string InstructorName,
    int Capacity,
    int ActiveEnrollments,
    int AvailableSeats);
