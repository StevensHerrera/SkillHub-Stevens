using SkillHub.Domain.Common;
using SkillHub.Domain.Enums;

namespace SkillHub.Domain.Entities;

public sealed class Enrollment
{
    private Enrollment()
    {
    }

    public Enrollment(Guid participantId, Guid workshopId, DateTime nowUtc)
    {
        if (participantId == Guid.Empty) throw new DomainException("The participant is required.");
        if (workshopId == Guid.Empty) throw new DomainException("The workshop is required.");

        ParticipantId = participantId;
        WorkshopId = workshopId;
        Status = EnrollmentStatus.Active;
        EnrolledAt = NormalizeUtc(nowUtc);
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ParticipantId { get; private set; }
    public Participant? Participant { get; private set; }
    public Guid WorkshopId { get; private set; }
    public Workshop? Workshop { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public DateTime EnrolledAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public Guid? CancelledByUserId { get; private set; }
    public string? CancellationReason { get; private set; }

    public bool IsActive => Status == EnrollmentStatus.Active;

    public void Cancel(Guid cancelledByUserId, DateTime nowUtc, string? reason)
    {
        if (!IsActive)
            throw new DomainException("Only an active enrollment can be cancelled.", DomainErrorKind.Conflict);
        if (cancelledByUserId == Guid.Empty)
            throw new DomainException("The user performing the cancellation is required.");

        Status = EnrollmentStatus.Cancelled;
        CancelledAt = NormalizeUtc(nowUtc);
        CancelledByUserId = cancelledByUserId;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
    }

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
