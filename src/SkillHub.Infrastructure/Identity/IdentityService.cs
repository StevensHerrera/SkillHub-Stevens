using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace SkillHub.Infrastructure.Identity;

public sealed class IdentityService(UserManager<ApplicationUser> userManager) : IIdentityService
{
    public async Task<AuthUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null) return null;
        var roles = await userManager.GetRolesAsync(user);
        return new AuthUser(user.Id, user.Email ?? string.Empty, user.FullName, roles.ToArray());
    }

    public async Task<AuthUser?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
        if (user is null) return null;

        if (await userManager.IsLockedOutAsync(user)) return null;

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return null;
        }

        await userManager.ResetAccessFailedCountAsync(user);
        var roles = await userManager.GetRolesAsync(user);
        return new AuthUser(user.Id, user.Email ?? string.Empty, user.FullName, roles.ToArray());
    }

    public async Task<(bool Succeeded, IReadOnlyList<string> Errors, AuthUser? User)> CreateUserAsync(
        string email,
        string password,
        string fullName,
        string role,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FullName = fullName.Trim(),
            EmailConfirmed = true
        };

        var created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
            return (false, created.Errors.Select(error => error.Description).ToArray(), null);

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return (false, roleResult.Errors.Select(error => error.Description).ToArray(), null);
        }

        var roles = await userManager.GetRolesAsync(user);
        return (true, Array.Empty<string>(),
            new AuthUser(user.Id, user.Email ?? normalizedEmail, user.FullName, roles.ToArray()));
    }

    public async Task<(bool Succeeded, IReadOnlyList<string> Errors)> UpdateUserAsync(
        Guid userId,
        string fullName,
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (false, ["User not found."]);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        user.FullName = fullName.Trim();
        user.Email = normalizedEmail;
        user.UserName = normalizedEmail;

        var result = await userManager.UpdateAsync(user);
        return (result.Succeeded, result.Errors.Select(error => error.Description).ToArray());
    }

    public async Task<bool> IsEmailInUseAsync(
        string email,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email.Trim().ToLowerInvariant());
        return user is not null && user.Id != excludeUserId;
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is not null) await userManager.DeleteAsync(user);
    }
}
