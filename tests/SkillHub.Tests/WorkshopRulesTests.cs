using SkillHub.Domain.Common;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;

namespace SkillHub.Tests;

public sealed class WorkshopRulesTests
{
    private static readonly DateTime Now = new(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid InstructorId = Guid.NewGuid();
    private static readonly Guid EmployeeId = Guid.NewGuid();

    private static Workshop CreateWorkshop(
        int capacity = 10,
        int startDaysFromNow = 30,
        int durationHours = 4) =>
        new(
            "TST-001",
            "Test workshop",
            "A workshop for unit tests",
            InstructorId,
            Now.AddDays(startDaysFromNow),
            Now.AddDays(startDaysFromNow).AddHours(durationHours),
            capacity,
            EmployeeId,
            Now);

    [Fact]
    public void Constructor_rejects_start_date_in_the_past()
    {
        var exception = Assert.Throws<DomainException>(() => new Workshop(
            "TST-001", "Title", "Description", InstructorId,
            Now.AddHours(-1), Now.AddHours(2), 10, EmployeeId, Now));

        Assert.Equal(DomainErrorKind.Conflict, exception.Kind);
    }

    [Fact]
    public void Constructor_rejects_end_date_before_start_date()
    {
        Assert.Throws<DomainException>(() => new Workshop(
            "TST-001", "Title", "Description", InstructorId,
            Now.AddDays(5), Now.AddDays(5).AddHours(-1), 10, EmployeeId, Now));
    }

    [Fact]
    public void Constructor_rejects_non_positive_capacity()
    {
        Assert.Throws<DomainException>(() => new Workshop(
            "TST-001", "Title", "Description", InstructorId,
            Now.AddDays(5), Now.AddDays(5).AddHours(2), 0, EmployeeId, Now));
    }

    [Fact]
    public void Enrollment_is_rejected_when_capacity_is_full()
    {
        var workshop = CreateWorkshop(capacity: 2);

        var exception = Assert.Throws<DomainException>(() =>
            workshop.EnsureEnrollmentIsAllowed(2, false, false, Now));

        Assert.Equal(DomainErrorKind.Conflict, exception.Kind);
        Assert.Contains("no available seats", exception.Message);
    }

    [Fact]
    public void Enrollment_is_rejected_when_participant_already_has_an_active_enrollment()
    {
        var workshop = CreateWorkshop();

        var exception = Assert.Throws<DomainException>(() =>
            workshop.EnsureEnrollmentIsAllowed(0, true, false, Now));

        Assert.Contains("already has an active enrollment", exception.Message);
    }

    [Fact]
    public void Enrollment_is_rejected_when_schedule_overlaps()
    {
        var workshop = CreateWorkshop();

        var exception = Assert.Throws<DomainException>(() =>
            workshop.EnsureEnrollmentIsAllowed(0, false, true, Now));

        Assert.Contains("overlaps", exception.Message);
    }

    [Fact]
    public void Enrollment_is_rejected_when_workshop_already_started()
    {
        var workshop = CreateWorkshop();

        var exception = Assert.Throws<DomainException>(() =>
            workshop.EnsureEnrollmentIsAllowed(0, false, false, workshop.StartAt));

        Assert.Contains("already started", exception.Message);
    }

    [Fact]
    public void Participant_cannot_cancel_with_less_than_24_hours()
    {
        var workshop = CreateWorkshop(startDaysFromNow: 1);

        Assert.False(workshop.AllowsParticipantCancellation(workshop.StartAt.AddHours(-23)));
        Assert.Throws<DomainException>(() =>
            workshop.EnsureParticipantCanCancel(workshop.StartAt.AddHours(-23)));
        Assert.True(workshop.AllowsParticipantCancellation(workshop.StartAt.AddHours(-25)));
    }

    [Fact]
    public void Employee_cannot_cancel_after_workshop_has_started()
    {
        var workshop = CreateWorkshop();

        Assert.Throws<DomainException>(() => workshop.EnsureEmployeeCanCancel(workshop.StartAt));
        workshop.EnsureEmployeeCanCancel(workshop.StartAt.AddMinutes(-1));
    }

    [Fact]
    public void Enrollment_cancel_records_who_and_when_and_cannot_be_repeated()
    {
        var enrollment = new Enrollment(Guid.NewGuid(), Guid.NewGuid(), Now);
        var cancelledBy = Guid.NewGuid();

        enrollment.Cancel(cancelledBy, Now.AddMinutes(5), "Schedule changed");

        Assert.Equal(EnrollmentStatus.Cancelled, enrollment.Status);
        Assert.False(enrollment.IsActive);
        Assert.Equal(cancelledBy, enrollment.CancelledByUserId);
        Assert.Equal(Now.AddMinutes(5), enrollment.CancelledAt);
        Assert.Equal("Schedule changed", enrollment.CancellationReason);
        Assert.Throws<DomainException>(() => enrollment.Cancel(cancelledBy, Now.AddMinutes(10), null));
    }

    [Fact]
    public void Available_seats_never_go_below_zero()
    {
        var workshop = CreateWorkshop(capacity: 5);

        Assert.Equal(3, workshop.AvailableSeats(2));
        Assert.Equal(0, workshop.AvailableSeats(8));
    }

    [Fact]
    public void Update_rejects_capacity_lower_than_active_enrollments()
    {
        var workshop = CreateWorkshop(capacity: 10);

        var exception = Assert.Throws<DomainException>(() => workshop.Update(
            workshop.Code, "New title", "New description", InstructorId,
            workshop.StartAt, workshop.EndAt, 2, 3, Now));

        Assert.Equal(DomainErrorKind.Conflict, exception.Kind);
        Assert.Contains("active enrollments", exception.Message);
    }

    [Fact]
    public void Update_rejects_start_date_in_the_past()
    {
        var workshop = CreateWorkshop();

        Assert.Throws<DomainException>(() => workshop.Update(
            workshop.Code, "New title", "New description", InstructorId,
            Now.AddHours(-1), Now.AddHours(2), 10, 0, Now));
    }

    [Fact]
    public void Update_changes_the_workshop_data()
    {
        var workshop = CreateWorkshop(capacity: 10);
        var newStart = Now.AddDays(60);
        var newEnd = newStart.AddHours(3);

        workshop.Update("NEW-001", "Updated title", "Updated description", InstructorId,
            newStart, newEnd, 25, 4, Now);

        Assert.Equal("NEW-001", workshop.Code);
        Assert.Equal("Updated title", workshop.Title);
        Assert.Equal(25, workshop.Capacity);
        Assert.Equal(newStart, workshop.StartAt);
        Assert.Equal(newEnd, workshop.EndAt);
    }

    [Fact]
    public void Instructor_and_participant_updates_normalize_email()
    {
        var instructor = new Instructor(Guid.NewGuid(), "Old Name", "old@correo.com", "Cooking");
        instructor.Update("New Name", "  NEW@Correo.com ", "Baking");

        var participant = new Participant(Guid.NewGuid(), "Old Name", "old@correo.com");
        participant.Update("New Name", "  NEW@Correo.com ");

        Assert.Equal("New Name", instructor.FullName);
        Assert.Equal("new@correo.com", instructor.Email);
        Assert.Equal("Baking", instructor.Specialty);
        Assert.Equal("new@correo.com", participant.Email);
    }
}
