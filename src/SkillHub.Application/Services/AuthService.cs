using SkillHub.Application.Common.Exceptions;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.Common.Models;
using SkillHub.Application.Dtos.Auth;
using SkillHub.Domain.Common;
using SkillHub.Domain.Entities;

namespace SkillHub.Application.Services;

public sealed class AuthService(
    IIdentityService identity,
    IParticipantRepository participants,
    IInstructorRepository instructors,
    IJwtTokenGenerator tokenGenerator,
    IUnitOfWork unitOfWork)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var fullName = request.FullName.Trim();

        if (await identity.FindByEmailAsync(email, cancellationToken) is not null)
            throw new ConflictException("An account with this email already exists.");

        var creation = await identity.CreateUserAsync(email, request.Password, fullName, Roles.Participant, cancellationToken);
        if (!creation.Succeeded || creation.User is null)
            throw new BadRequestException(string.Join(" ", creation.Errors));

        try
        {
            var participant = new Participant(creation.User.Id, fullName, email);
            await participants.AddAsync(participant, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return BuildResponse(creation.User, participant.Id, null);
        }
        catch
        {
            await identity.DeleteUserAsync(creation.User.Id, cancellationToken);
            throw;
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await identity.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (user is null)
            throw new UnauthorizedException("Invalid email or password.");

        var participant = await participants.GetByUserIdAsync(user.Id, cancellationToken);
        var instructor = participant is null
            ? await instructors.GetByUserIdAsync(user.Id, cancellationToken)
            : null;

        return BuildResponse(user, participant?.Id, instructor?.Id);
    }

    private AuthResponse BuildResponse(AuthUser user, Guid? participantId, Guid? instructorId)
    {
        var token = tokenGenerator.Generate(user, participantId, instructorId);
        return new AuthResponse(token.AccessToken, token.ExpiresAt, user.Email, user.FullName, user.Roles.ToArray());
    }
}
