using SkillHub.Domain.Entities;

namespace SkillHub.Application.Common.Interfaces;

public interface IParticipantRepository
{
    Task<Participant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Participant?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Participant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Participant>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid? excludeParticipantId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Participant participant, CancellationToken cancellationToken = default);
}
