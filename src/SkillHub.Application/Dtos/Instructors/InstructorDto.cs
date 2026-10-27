namespace SkillHub.Application.Dtos.Instructors;

public sealed record InstructorDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string Specialty);
