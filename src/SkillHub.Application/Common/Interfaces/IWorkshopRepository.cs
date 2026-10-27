using SkillHub.Application.Common.Models;
using SkillHub.Domain.Entities;

namespace SkillHub.Application.Common.Interfaces;

public interface IWorkshopRepository
{
    Task<Workshop?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Workshop?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsExcludingAsync(string code, Guid excludeWorkshopId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkshopWithCount>> GetUpcomingAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
    Task<WorkshopWithCount?> GetByIdWithCountAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkshopWithCount>> GetTopAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Workshop workshop, CancellationToken cancellationToken = default);
}
