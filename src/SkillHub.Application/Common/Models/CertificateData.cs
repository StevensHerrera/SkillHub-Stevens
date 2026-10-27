namespace SkillHub.Application.Common.Models;

public sealed record CertificateData(
    string ParticipantName,
    string ParticipantEmail,
    string WorkshopCode,
    string WorkshopTitle,
    string InstructorName,
    DateTime StartAt,
    DateTime EndAt,
    DateTime EnrolledAt,
    DateTime GeneratedAt);
