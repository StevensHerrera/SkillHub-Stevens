using SkillHub.Application.Common;
using SkillHub.Application.Common.Exceptions;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.Common.Mappings;
using SkillHub.Application.Dtos.Instructors;
using SkillHub.Domain.Common;
using SkillHub.Domain.Entities;

namespace SkillHub.Application.Services;

public sealed class InstructorService(
    IInstructorRepository instructors,
    IIdentityService identity,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<InstructorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        EnsureEmployee();
        var items = await instructors.GetAllAsync(cancellationToken);
        return items.Select(instructor => instructor.ToDto()).ToArray();
    }

    public async Task<InstructorDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureEmployee();
        var instructor = await instructors.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Instructor not found.");
        return instructor.ToDto();
    }

    public async Task<InstructorDto> CreateAsync(
        CreateInstructorRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureEmployee();
        var email = request.Email.Trim().ToLowerInvariant();

        if (await identity.IsEmailInUseAsync(email, null, cancellationToken))
            throw new ConflictException("An account with this email already exists.");

        var password = string.IsNullOrWhiteSpace(request.Password)
            ? DefaultCredentials.InstructorPassword
            : request.Password;

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var creation = await identity.CreateUserAsync(
                email, password, request.FullName, Roles.Employee, cancellationToken);
            if (!creation.Succeeded || creation.User is null)
                throw new BadRequestException(string.Join(" ", creation.Errors));

            var instructor = new Instructor(creation.User.Id, request.FullName, email, request.Specialty);
            await instructors.AddAsync(instructor, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return instructor.ToDto();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<InstructorDto> UpdateAsync(
        Guid id,
        UpdateInstructorRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureEmployee();
        var instructor = await instructors.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Instructor not found.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (!string.Equals(email, instructor.Email, StringComparison.OrdinalIgnoreCase)
            && await identity.IsEmailInUseAsync(email, instructor.UserId, cancellationToken))
            throw new ConflictException("An account with this email already exists.");

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var update = await identity.UpdateUserAsync(
                instructor.UserId, request.FullName, email, cancellationToken);
            if (!update.Succeeded)
                throw new BadRequestException(string.Join(" ", update.Errors));

            instructor.Update(request.FullName, email, request.Specialty);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return instructor.ToDto();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private void EnsureEmployee()
    {
        if (!currentUser.IsEmployee)
            throw new ForbiddenException("Only SkillHub employees can manage instructors.");
    }
}
