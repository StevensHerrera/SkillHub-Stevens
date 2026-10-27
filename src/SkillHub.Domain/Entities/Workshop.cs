using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities;

public sealed class Workshop
{
    public const int MinimumCapacity = 1;
    public const int MaximumCapacity = 500;
    public static readonly TimeSpan ParticipantCancellationWindow = TimeSpan.FromHours(24);

    private Workshop()
    {
    }

    public Workshop(
        string code,
        string title,
        string description,
        Guid instructorId,
        DateTime startAtUtc,
        DateTime endAtUtc,
        int capacity,
        Guid createdByUserId,
        DateTime nowUtc)
    {
        if (instructorId == Guid.Empty) throw new DomainException("The workshop instructor is required.");
        if (createdByUserId == Guid.Empty) throw new DomainException("The creating employee is required.");

        Code = Require(code, "Code", 40).ToUpperInvariant();
        Title = Require(title, "Title", 200);
        Description = Require(description, "Description", 2000);
        InstructorId = instructorId;
        StartAt = NormalizeUtc(startAtUtc);
        EndAt = NormalizeUtc(endAtUtc);
        Capacity = ValidateCapacity(capacity);
        CreatedByUserId = createdByUserId;
        CreatedAt = NormalizeUtc(nowUtc);

        EnsureScheduleIsValid();
        if (StartAt <= CreatedAt)
            throw new DomainException("A workshop must be created with a future start date.", DomainErrorKind.Conflict);
    }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid InstructorId { get; private set; }
    public Instructor? Instructor { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public int Capacity { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public ICollection<Enrollment> Enrollments { get; private set; } = new List<Enrollment>();

    public bool HasStarted(DateTime nowUtc) => NormalizeUtc(nowUtc) >= StartAt;

    public bool AllowsParticipantCancellation(DateTime nowUtc) =>
        !HasStarted(nowUtc) && NormalizeUtc(nowUtc) <= StartAt.Subtract(ParticipantCancellationWindow);

    public int AvailableSeats(int activeEnrollments) => Math.Max(0, Capacity - activeEnrollments);

    public void Update(
        string code,
        string title,
        string description,
        Guid instructorId,
        DateTime startAtUtc,
        DateTime endAtUtc,
        int capacity,
        int activeEnrollments,
        DateTime nowUtc)
    {
        if (instructorId == Guid.Empty) throw new DomainException("The workshop instructor is required.");

        var startAt = NormalizeUtc(startAtUtc);
        var endAt = NormalizeUtc(endAtUtc);

        if (endAt <= startAt)
            throw new DomainException("The workshop end date must be after its start date.");

        if (startAt <= NormalizeUtc(nowUtc))
            throw new DomainException("The workshop start date must remain in the future.", DomainErrorKind.Conflict);

        if (capacity < activeEnrollments)
            throw new DomainException(
                $"Capacity cannot be lower than the current {activeEnrollments} active enrollments.",
                DomainErrorKind.Conflict);

        Code = Require(code, "Code", 40).ToUpperInvariant();
        Title = Require(title, "Title", 200);
        Description = Require(description, "Description", 2000);
        InstructorId = instructorId;
        StartAt = startAt;
        EndAt = endAt;
        Capacity = ValidateCapacity(capacity);
    }

    public void EnsureEnrollmentIsAllowed(
        int activeEnrollments,
        bool hasActiveEnrollment,
        bool hasScheduleConflict,
        DateTime nowUtc)
    {
        if (hasActiveEnrollment)
            throw new DomainException("The participant already has an active enrollment in this workshop.", DomainErrorKind.Conflict);

        if (HasStarted(nowUtc))
            throw new DomainException("The workshop has already started and cannot receive new enrollments.", DomainErrorKind.Conflict);

        if (activeEnrollments >= Capacity)
            throw new DomainException("The workshop has no available seats.", DomainErrorKind.Conflict);

        if (hasScheduleConflict)
            throw new DomainException("The participant already has an active enrollment that overlaps this workshop schedule.", DomainErrorKind.Conflict);
    }

    public void EnsureParticipantCanCancel(DateTime nowUtc)
    {
        if (HasStarted(nowUtc))
            throw new DomainException("The workshop has already started; the enrollment cannot be cancelled.", DomainErrorKind.Conflict);

        if (!AllowsParticipantCancellation(nowUtc))
            throw new DomainException(
                "A participant can only cancel an enrollment up to 24 hours before the workshop starts.",
                DomainErrorKind.Conflict);
    }

    public void EnsureEmployeeCanCancel(DateTime nowUtc)
    {
        if (HasStarted(nowUtc))
            throw new DomainException("The workshop has already started; the enrollment cannot be cancelled.", DomainErrorKind.Conflict);
    }

    private void EnsureScheduleIsValid()
    {
        if (EndAt <= StartAt)
            throw new DomainException("The workshop end date must be after its start date.");
    }

    private static int ValidateCapacity(int capacity)
    {
        if (capacity < MinimumCapacity || capacity > MaximumCapacity)
            throw new DomainException($"Capacity must be between {MinimumCapacity} and {MaximumCapacity}.");
        return capacity;
    }

    private static string Require(string value, string field, int maxLength)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0) throw new DomainException($"{field} is required.");
        if (trimmed.Length > maxLength) throw new DomainException($"{field} cannot exceed {maxLength} characters.");
        return trimmed;
    }

    private static DateTime NormalizeUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
