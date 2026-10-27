using SkillHub.Application.Common;
using SkillHub.Application.Common.Exceptions;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.Common.Mappings;
using SkillHub.Application.Dtos.Participants;
using SkillHub.Domain.Common;
using SkillHub.Domain.Entities;

namespace SkillHub.Application.Services;

public sealed class ParticipantService(
    IParticipantRepository participants,
    IIdentityService identity,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
{
    public async Task<IReadOnlyList<ParticipantDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        EnsureEmployee();
        var items = await participants.GetAllAsync(cancellationToken);
        return items.Select(participant => participant.ToDto()).ToArray();
    }

    public async Task<ParticipantDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureEmployee();
        var participant = await participants.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Participant not found.");
        return participant.ToDto();
    }

    public async Task<ParticipantDto> CreateAsync(
        CreateParticipantRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureEmployee();
        var email = request.Email.Trim().ToLowerInvariant();

        if (await identity.IsEmailInUseAsync(email, null, cancellationToken))
            throw new ConflictException("An account with this email already exists.");

        var password = string.IsNullOrWhiteSpace(request.Password)
            ? DefaultCredentials.ParticipantPassword
            : request.Password;

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var creation = await identity.CreateUserAsync(
                email, password, request.FullName, Roles.Participant, cancellationToken);
            if (!creation.Succeeded || creation.User is null)
                throw new BadRequestException(string.Join(" ", creation.Errors));

            var participant = new Participant(creation.User.Id, request.FullName, email);
            await participants.AddAsync(participant, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return participant.ToDto();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ParticipantDto> UpdateAsync(
        Guid id,
        UpdateParticipantRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureEmployee();
        var participant = await participants.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Participant not found.");

        var email = request.Email.Trim().ToLowerInvariant();
        if (!string.Equals(email, participant.Email, StringComparison.OrdinalIgnoreCase)
            && await identity.IsEmailInUseAsync(email, participant.UserId, cancellationToken))
            throw new ConflictException("An account with this email already exists.");

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var update = await identity.UpdateUserAsync(
                participant.UserId, request.FullName, email, cancellationToken);
            if (!update.Succeeded)
                throw new BadRequestException(string.Join(" ", update.Errors));

            participant.Update(request.FullName, email);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return participant.ToDto();
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
            throw new ForbiddenException("Only SkillHub employees can manage participants.");
    }
}
