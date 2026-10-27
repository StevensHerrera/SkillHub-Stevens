using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Domain.Entities;

namespace SkillHub.Infrastructure.Persistence.Repositories;

public sealed class InstructorRepository(SkillHubDbContext db) : IInstructorRepository
{
    public Task<Instructor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Instructors.FirstOrDefaultAsync(instructor => instructor.Id == id, cancellationToken);

    public Task<Instructor?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Instructors.FirstOrDefaultAsync(instructor => instructor.UserId == userId, cancellationToken);

    public Task<Instructor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return db.Instructors.FirstOrDefaultAsync(instructor => instructor.Email == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Instructor>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Instructors
            .AsNoTracking()
            .OrderBy(instructor => instructor.FullName)
            .ToListAsync(cancellationToken);

    public Task<bool> EmailExistsAsync(
        string email,
        Guid? excludeInstructorId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return db.Instructors.AnyAsync(
            instructor => instructor.Email == normalized
                && (excludeInstructorId == null || instructor.Id != excludeInstructorId),
            cancellationToken);
    }

    public Task AddAsync(Instructor instructor, CancellationToken cancellationToken = default) =>
        db.Instructors.AddAsync(instructor, cancellationToken).AsTask();
}
