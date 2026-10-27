using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.Dtos.Participants;

public sealed record UpdateParticipantRequest(
    [Required, StringLength(200, MinimumLength = 3)] string FullName,
    [Required, EmailAddress, StringLength(256)] string Email);
