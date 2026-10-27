using Microsoft.EntityFrameworkCore.Storage;
using SkillHub.Application.Common.Interfaces;

namespace SkillHub.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork(SkillHubDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    public async Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        new AppTransaction(await db.Database.BeginTransactionAsync(cancellationToken));

    private sealed class AppTransaction(IDbContextTransaction transaction) : IAppTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) =>
            transaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken = default) =>
            transaction.RollbackAsync(cancellationToken);

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
