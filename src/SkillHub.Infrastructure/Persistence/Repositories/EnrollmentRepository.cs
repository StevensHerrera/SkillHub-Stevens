using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;

namespace SkillHub.Infrastructure.Persistence.Repositories;

public sealed class EnrollmentRepository(SkillHubDbContext db) : IEnrollmentRepository
{
    public Task<Enrollment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Enrollments
            .Include(enrollment => enrollment.Participant)
            .Include(enrollment => enrollment.Workshop)
                .ThenInclude(workshop => workshop!.Instructor)
            .FirstOrDefaultAsync(enrollment => enrollment.Id == id, cancellationToken);

    public Task<int> CountActiveForWorkshopAsync(Guid workshopId, CancellationToken cancellationToken = default) =>
        db.Enrollments.CountAsync(
            enrollment => enrollment.WorkshopId == workshopId && enrollment.Status == EnrollmentStatus.Active,
            cancellationToken);

    public Task<bool> HasActiveEnrollmentAsync(
        Guid participantId,
        Guid workshopId,
        CancellationToken cancellationToken = default) =>
        db.Enrollments.AnyAsync(
            enrollment => enrollment.ParticipantId == participantId
                && enrollment.WorkshopId == workshopId
                && enrollment.Status == EnrollmentStatus.Active,
            cancellationToken);

    public Task<bool> HasScheduleConflictAsync(
        Guid participantId,
        DateTime startAt,
        DateTime endAt,
        CancellationToken cancellationToken = default) =>
        db.Enrollments.AnyAsync(
            enrollment => enrollment.ParticipantId == participantId
                && enrollment.Status == EnrollmentStatus.Active
                && enrollment.Workshop!.StartAt < endAt
                && enrollment.Workshop.EndAt > startAt,
            cancellationToken);

    public Task<bool> HasScheduleConflictExcludingWorkshopAsync(
        Guid participantId,
        DateTime startAt,
        DateTime endAt,
        Guid excludedWorkshopId,
        CancellationToken cancellationToken = default) =>
        db.Enrollments.AnyAsync(
            enrollment => enrollment.ParticipantId == participantId
                && enrollment.Status == EnrollmentStatus.Active
                && enrollment.WorkshopId != excludedWorkshopId
                && enrollment.Workshop!.StartAt < endAt
                && enrollment.Workshop.EndAt > startAt,
            cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetActiveParticipantIdsAsync(
        Guid workshopId,
        CancellationToken cancellationToken = default) =>
        await db.Enrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.WorkshopId == workshopId
                && enrollment.Status == EnrollmentStatus.Active)
            .Select(enrollment => enrollment.ParticipantId)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Enrollment>> GetForParticipantAsync(
        Guid participantId,
        CancellationToken cancellationToken = default) =>
        await db.Enrollments
            .AsNoTracking()
            .Include(enrollment => enrollment.Workshop)
                .ThenInclude(workshop => workshop!.Instructor)
            .Where(enrollment => enrollment.ParticipantId == participantId)
            .OrderByDescending(enrollment => enrollment.EnrolledAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Enrollment>> GetForWorkshopAsync(
        Guid workshopId,
        CancellationToken cancellationToken = default) =>
        await db.Enrollments
            .AsNoTracking()
            .Include(enrollment => enrollment.Participant)
            .Where(enrollment => enrollment.WorkshopId == workshopId)
            .OrderBy(enrollment => enrollment.EnrolledAt)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default) =>
        db.Enrollments.AddAsync(enrollment, cancellationToken).AsTask();
}
