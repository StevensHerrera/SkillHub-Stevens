using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Domain.Entities;

namespace SkillHub.Infrastructure.Persistence.Repositories;

public sealed class ParticipantRepository(SkillHubDbContext db) : IParticipantRepository
{
    public Task<Participant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Participants.FirstOrDefaultAsync(participant => participant.Id == id, cancellationToken);

    public Task<Participant?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Participants.FirstOrDefaultAsync(participant => participant.UserId == userId, cancellationToken);

    public Task<Participant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return db.Participants.FirstOrDefaultAsync(participant => participant.Email == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Participant>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Participants
            .AsNoTracking()
            .OrderBy(participant => participant.FullName)
            .ToListAsync(cancellationToken);

    public Task<bool> EmailExistsAsync(
        string email,
        Guid? excludeParticipantId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return db.Participants.AnyAsync(
            participant => participant.Email == normalized
                && (excludeParticipantId == null || participant.Id != excludeParticipantId),
            cancellationToken);
    }

    public Task AddAsync(Participant participant, CancellationToken cancellationToken = default) =>
        db.Participants.AddAsync(participant, cancellationToken).AsTask();
}
