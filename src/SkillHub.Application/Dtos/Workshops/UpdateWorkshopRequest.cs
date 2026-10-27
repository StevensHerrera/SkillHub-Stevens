using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.Dtos.Workshops;

public sealed record UpdateWorkshopRequest(
    [Required, StringLength(40, MinimumLength = 2)] string Code,
    [Required, StringLength(200, MinimumLength = 3)] string Title,
    [Required, StringLength(2000, MinimumLength = 3)] string Description,
    [Required, EmailAddress, StringLength(256)] string InstructorEmail,
    [Required] DateTimeOffset StartAt,
    [Required] DateTimeOffset EndAt,
    [Range(1, 500)] int Capacity);
