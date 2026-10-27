using SkillHub.Application.Common.Exceptions;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.Common.Mappings;
using SkillHub.Application.Common.Models;
using SkillHub.Application.Dtos.Enrollments;
using SkillHub.Domain.Common;
using SkillHub.Domain.Entities;

namespace SkillHub.Application.Services;

public sealed class EnrollmentService(
    IEnrollmentRepository enrollments,
    IWorkshopRepository workshops,
    IParticipantRepository participants,
    IEnrollmentExcelParser excelParser,
    ICertificatePdfGenerator certificateGenerator,
    ICurrentUserService currentUser,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<EnrollmentDto> EnrollAsync(CreateEnrollmentRequest request, CancellationToken cancellationToken = default)
    {
        var participant = await GetCurrentParticipantAsync(cancellationToken);
        var workshop = await workshops.GetByIdAsync(request.WorkshopId, cancellationToken)
            ?? throw new NotFoundException("Workshop not found.");

        var enrollment = await EnrollParticipantAsync(participant, workshop, cancellationToken);
        var created = await enrollments.GetByIdWithDetailsAsync(enrollment.Id, cancellationToken)
            ?? throw new NotFoundException("Enrollment not found after creation.");
        return created.ToDto();
    }

    public async Task<IReadOnlyList<EnrollmentDto>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var participant = await GetCurrentParticipantAsync(cancellationToken);
        var items = await enrollments.GetForParticipantAsync(participant.Id, cancellationToken);
        return items.Select(item => item.ToDto()).ToArray();
    }

    public async Task<EnrollmentDto> CancelAsync(
        Guid enrollmentId,
        CancelEnrollmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
            throw new UnauthorizedException("Authentication is required.");

        var enrollment = await enrollments.GetByIdWithDetailsAsync(enrollmentId, cancellationToken)
            ?? throw new NotFoundException("Enrollment not found.");

        var isOwner = enrollment.Participant?.UserId == currentUser.UserId;
        if (!isOwner && !currentUser.IsEmployee)
            throw new ForbiddenException("You can only cancel your own enrollments.");

        var workshop = enrollment.Workshop
            ?? await workshops.GetByIdAsync(enrollment.WorkshopId, cancellationToken)
            ?? throw new NotFoundException("Workshop not found.");

        var now = clock.UtcNow;
        if (currentUser.IsEmployee)
            workshop.EnsureEmployeeCanCancel(now);
        else
            workshop.EnsureParticipantCanCancel(now);

        enrollment.Cancel(currentUser.UserId.Value, now, request.Reason);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return enrollment.ToDto();
    }

    public async Task<IReadOnlyList<WorkshopEnrollmentDto>> GetWorkshopEnrollmentsAsync(
        Guid workshopId,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsEmployee)
            throw new ForbiddenException("Only SkillHub employees can consult workshop enrollments.");

        _ = await workshops.GetByIdAsync(workshopId, cancellationToken)
            ?? throw new NotFoundException("Workshop not found.");

        var items = await enrollments.GetForWorkshopAsync(workshopId, cancellationToken);
        return items.Select(item => item.ToWorkshopEnrollmentDto()).ToArray();
    }

    public async Task<(byte[] Content, string FileName)> GetCertificateAsync(
        Guid enrollmentId,
        CancellationToken cancellationToken = default)
    {
        if (currentUser.UserId is null)
            throw new UnauthorizedException("Authentication is required.");

        var enrollment = await enrollments.GetByIdWithDetailsAsync(enrollmentId, cancellationToken)
            ?? throw new NotFoundException("Enrollment not found.");

        if (!enrollment.IsActive)
            throw new ConflictException("A cancelled enrollment does not have a certificate.");

        var isOwner = enrollment.Participant?.UserId == currentUser.UserId;
        if (!isOwner && !currentUser.IsEmployee)
            throw new ForbiddenException("You can only download your own certificate.");

        var workshop = enrollment.Workshop!;
        var participant = enrollment.Participant!;

        var data = new CertificateData(
            participant.FullName,
            participant.Email,
            workshop.Code,
            workshop.Title,
            workshop.Instructor?.FullName ?? string.Empty,
            AsUtc(workshop.StartAt),
            AsUtc(workshop.EndAt),
            AsUtc(enrollment.EnrolledAt),
            clock.UtcNow);

        var content = certificateGenerator.Generate(data);
        return (content, $"skillhub-enrollment-{workshop.Code}.pdf");
    }

    public async Task<ImportEnrollmentsResponse> ImportAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsEmployee)
            throw new ForbiddenException("Only SkillHub employees can import enrollments.");

        var rows = excelParser.Parse(stream);
        var errors = new List<ImportRowError>();
        var enrolled = 0;

        foreach (var row in rows)
        {
            try
            {
                var participant = await participants.GetByEmailAsync(row.ParticipantEmail, cancellationToken)
                    ?? throw new BadRequestException($"Participant '{row.ParticipantEmail}' was not found.");

                var workshop = await workshops.GetByCodeAsync(row.WorkshopCode, cancellationToken)
                    ?? throw new BadRequestException($"Workshop '{row.WorkshopCode}' was not found.");

                await EnrollParticipantAsync(participant, workshop, cancellationToken);
                enrolled++;
            }
            catch (DomainException exception)
            {
                errors.Add(new ImportRowError(row.RowNumber, exception.Message));
            }
            catch (AppException exception)
            {
                errors.Add(new ImportRowError(row.RowNumber, exception.Message));
            }
        }

        return new ImportEnrollmentsResponse(rows.Count, enrolled, errors.Count, errors);
    }

    private async Task<Enrollment> EnrollParticipantAsync(
        Participant participant,
        Workshop workshop,
        CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var activeCount = await enrollments.CountActiveForWorkshopAsync(workshop.Id, cancellationToken);
        var hasActive = await enrollments.HasActiveEnrollmentAsync(participant.Id, workshop.Id, cancellationToken);
        var hasConflict = await enrollments.HasScheduleConflictAsync(
            participant.Id, workshop.StartAt, workshop.EndAt, cancellationToken);

        workshop.EnsureEnrollmentIsAllowed(activeCount, hasActive, hasConflict, now);

        var enrollment = new Enrollment(participant.Id, workshop.Id, now);
        await enrollments.AddAsync(enrollment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return enrollment;
    }

    private async Task<Participant> GetCurrentParticipantAsync(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            throw new UnauthorizedException("Authentication is required.");

        var participant = await participants.GetByUserIdAsync(currentUser.UserId.Value, cancellationToken);
        if (participant is null)
            throw new ForbiddenException("Only participants can manage enrollments.");
        return participant;
    }

    private static DateTime AsUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
