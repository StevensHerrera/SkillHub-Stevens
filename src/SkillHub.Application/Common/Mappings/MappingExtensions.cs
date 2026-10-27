using SkillHub.Application.Common.Models;
using SkillHub.Application.Dtos.Enrollments;
using SkillHub.Application.Dtos.Instructors;
using SkillHub.Application.Dtos.Participants;
using SkillHub.Application.Dtos.Reports;
using SkillHub.Application.Dtos.Workshops;
using SkillHub.Domain.Entities;

namespace SkillHub.Application.Common.Mappings;

public static class MappingExtensions
{
    public static WorkshopDto ToDto(this WorkshopWithCount source)
    {
        var workshop = source.Workshop;
        return new WorkshopDto(
            workshop.Id,
            workshop.Code,
            workshop.Title,
            workshop.Description,
            workshop.Instructor?.FullName ?? string.Empty,
            workshop.Instructor?.Email ?? string.Empty,
            workshop.Instructor?.Specialty ?? string.Empty,
            ToUtc(workshop.StartAt),
            ToUtc(workshop.EndAt),
            workshop.Capacity,
            source.ActiveEnrollments,
            workshop.AvailableSeats(source.ActiveEnrollments));
    }

    public static EnrollmentDto ToDto(this Enrollment enrollment)
    {
        var workshop = enrollment.Workshop;
        return new EnrollmentDto(
            enrollment.Id,
            enrollment.WorkshopId,
            workshop?.Code ?? string.Empty,
            workshop?.Title ?? string.Empty,
            workshop?.Instructor?.FullName ?? string.Empty,
            ToUtc(workshop?.StartAt ?? default),
            ToUtc(workshop?.EndAt ?? default),
            enrollment.Status.ToString(),
            ToUtc(enrollment.EnrolledAt),
            enrollment.CancelledAt is null ? null : ToUtc(enrollment.CancelledAt.Value),
            enrollment.CancellationReason);
    }

    public static WorkshopEnrollmentDto ToWorkshopEnrollmentDto(this Enrollment enrollment)
    {
        var participant = enrollment.Participant;
        return new WorkshopEnrollmentDto(
            enrollment.Id,
            enrollment.ParticipantId,
            participant?.FullName ?? string.Empty,
            participant?.Email ?? string.Empty,
            enrollment.Status.ToString(),
            ToUtc(enrollment.EnrolledAt),
            enrollment.CancelledAt is null ? null : ToUtc(enrollment.CancelledAt.Value),
            enrollment.CancellationReason);
    }

    public static InstructorDto ToDto(this Instructor instructor) =>
        new(instructor.Id, instructor.UserId, instructor.FullName, instructor.Email, instructor.Specialty);

    public static ParticipantDto ToDto(this Participant participant) =>
        new(participant.Id, participant.UserId, participant.FullName, participant.Email);

    public static TopWorkshopDto ToTopDto(this WorkshopWithCount source)
    {
        var workshop = source.Workshop;
        return new TopWorkshopDto(
            workshop.Id,
            workshop.Code,
            workshop.Title,
            workshop.Instructor?.FullName ?? string.Empty,
            workshop.Capacity,
            source.ActiveEnrollments,
            workshop.AvailableSeats(source.ActiveEnrollments));
    }

    private static DateTime ToUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
