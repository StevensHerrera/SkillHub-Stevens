using SkillHub.Domain.Entities;

namespace SkillHub.Application.Common.Interfaces;

public interface IInstructorRepository
{
    Task<Instructor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Instructor?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Instructor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Instructor>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid? excludeInstructorId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Instructor instructor, CancellationToken cancellationToken = default);
}
