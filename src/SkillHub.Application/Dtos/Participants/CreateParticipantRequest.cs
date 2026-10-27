using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.Dtos.Participants;

public sealed record CreateParticipantRequest(
    [Required, StringLength(200, MinimumLength = 3)] string FullName,
    [Required, EmailAddress, StringLength(256)] string Email,
    [StringLength(100, MinimumLength = 10)] string? Password);
