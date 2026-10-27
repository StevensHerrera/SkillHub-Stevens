using SkillHub.Application.Common.Models;

namespace SkillHub.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<AuthUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<(bool Succeeded, IReadOnlyList<string> Errors, AuthUser? User)> CreateUserAsync(
        string email,
        string password,
        string fullName,
        string role,
        CancellationToken cancellationToken = default);

    Task<(bool Succeeded, IReadOnlyList<string> Errors)> UpdateUserAsync(
        Guid userId,
        string fullName,
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> IsEmailInUseAsync(
        string email,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default);

    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
