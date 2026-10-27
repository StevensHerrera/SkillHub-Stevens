using SkillHub.Application.Common.Exceptions;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.Common.Mappings;
using SkillHub.Application.Dtos.Workshops;
using SkillHub.Domain.Entities;

namespace SkillHub.Application.Services;

public sealed class WorkshopService(
    IWorkshopRepository workshops,
    IInstructorRepository instructors,
    IEnrollmentRepository enrollments,
    ICurrentUserService currentUser,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<WorkshopDto>> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var upcoming = await workshops.GetUpcomingAsync(clock.UtcNow, cancellationToken);
        return upcoming.Select(item => item.ToDto()).ToArray();
    }

    public async Task<WorkshopDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workshop = await workshops.GetByIdWithCountAsync(id, cancellationToken)
            ?? throw new NotFoundException("Workshop not found.");
        return workshop.ToDto();
    }

    public async Task<WorkshopDto> CreateAsync(CreateWorkshopRequest request, CancellationToken cancellationToken = default)
    {
        EnsureEmployee("Only SkillHub employees can create workshops.");

        var instructor = await ResolveInstructorAsync(request.InstructorEmail, cancellationToken);
        var code = request.Code.Trim().ToUpperInvariant();

        if (await workshops.CodeExistsAsync(code, cancellationToken))
            throw new ConflictException($"A workshop with code '{code}' already exists.");

        var workshop = new Workshop(
            code,
            request.Title,
            request.Description,
            instructor.Id,
            request.StartAt.UtcDateTime,
            request.EndAt.UtcDateTime,
            request.Capacity,
            currentUser.UserId!.Value,
            clock.UtcNow);

        await workshops.AddAsync(workshop, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(workshop.Id, cancellationToken);
    }

    public async Task<WorkshopDto> UpdateAsync(
        Guid id,
        UpdateWorkshopRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureEmployee("Only SkillHub employees can update workshops.");

        var workshop = await workshops.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Workshop not found.");

        var instructor = await ResolveInstructorAsync(request.InstructorEmail, cancellationToken);
        var code = request.Code.Trim().ToUpperInvariant();

        if (!string.Equals(code, workshop.Code, StringComparison.OrdinalIgnoreCase)
            && await workshops.CodeExistsExcludingAsync(code, id, cancellationToken))
            throw new ConflictException($"A workshop with code '{code}' already exists.");

        var activeEnrollments = await enrollments.CountActiveForWorkshopAsync(id, cancellationToken);

        workshop.Update(
            code,
            request.Title,
            request.Description,
            instructor.Id,
            request.StartAt.UtcDateTime,
            request.EndAt.UtcDateTime,
            request.Capacity,
            activeEnrollments,
            clock.UtcNow);

        var participantIds = await enrollments.GetActiveParticipantIdsAsync(id, cancellationToken);
        foreach (var participantId in participantIds)
        {
            if (await enrollments.HasScheduleConflictExcludingWorkshopAsync(
                    participantId, workshop.StartAt, workshop.EndAt, id, cancellationToken))
                throw new ConflictException(
                    "The new workshop schedule conflicts with another active enrollment of an enrolled participant.");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task<Instructor> ResolveInstructorAsync(
        string instructorEmail,
        CancellationToken cancellationToken)
    {
        var email = instructorEmail.Trim().ToLowerInvariant();
        return await instructors.GetByEmailAsync(email, cancellationToken)
            ?? throw new NotFoundException($"The instructor '{email}' is not registered in SkillHub.");
    }

    private void EnsureEmployee(string message)
    {
        if (!currentUser.IsEmployee || currentUser.UserId is null)
            throw new ForbiddenException(message);
    }
}
