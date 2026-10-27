using SkillHub.Domain.Entities;

namespace SkillHub.Application.Common.Interfaces;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountActiveForWorkshopAsync(Guid workshopId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveEnrollmentAsync(Guid participantId, Guid workshopId, CancellationToken cancellationToken = default);
    Task<bool> HasScheduleConflictAsync(Guid participantId, DateTime startAt, DateTime endAt, CancellationToken cancellationToken = default);
    Task<bool> HasScheduleConflictExcludingWorkshopAsync(
        Guid participantId,
        DateTime startAt,
        DateTime endAt,
        Guid excludedWorkshopId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetActiveParticipantIdsAsync(Guid workshopId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Enrollment>> GetForParticipantAsync(Guid participantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Enrollment>> GetForWorkshopAsync(Guid workshopId, CancellationToken cancellationToken = default);
    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
}
