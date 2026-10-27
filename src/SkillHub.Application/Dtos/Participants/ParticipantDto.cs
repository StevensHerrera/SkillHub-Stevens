namespace SkillHub.Application.Dtos.Participants;

public sealed record ParticipantDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email);
