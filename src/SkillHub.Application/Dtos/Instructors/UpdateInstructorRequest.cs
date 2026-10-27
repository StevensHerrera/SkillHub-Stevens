using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.Dtos.Instructors;

public sealed record UpdateInstructorRequest(
    [Required, StringLength(200, MinimumLength = 3)] string FullName,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(150, MinimumLength = 2)] string Specialty);
