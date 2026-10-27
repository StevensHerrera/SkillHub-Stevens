using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.Common.Models;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;

namespace SkillHub.Infrastructure.Persistence.Repositories;

public sealed class WorkshopRepository(SkillHubDbContext db) : IWorkshopRepository
{
    public Task<Workshop?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Workshops
            .Include(workshop => workshop.Instructor)
            .FirstOrDefaultAsync(workshop => workshop.Id == id, cancellationToken);

    public Task<Workshop?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return db.Workshops
            .Include(workshop => workshop.Instructor)
            .FirstOrDefaultAsync(workshop => workshop.Code == normalized, cancellationToken);
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return db.Workshops.AnyAsync(workshop => workshop.Code == normalized, cancellationToken);
    }

    public Task<bool> CodeExistsExcludingAsync(
        string code,
        Guid excludeWorkshopId,
        CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return db.Workshops.AnyAsync(
            workshop => workshop.Code == normalized && workshop.Id != excludeWorkshopId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<WorkshopWithCount>> GetUpcomingAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        var items = await db.Workshops
            .AsNoTracking()
            .Include(workshop => workshop.Instructor)
            .Where(workshop => workshop.StartAt > nowUtc)
            .OrderBy(workshop => workshop.StartAt)
            .ToListAsync(cancellationToken);

        var counts = await ActiveCountsAsync(cancellationToken);
        return items.Select(workshop => new WorkshopWithCount(workshop, counts.GetValueOrDefault(workshop.Id))).ToArray();
    }

    public async Task<WorkshopWithCount?> GetByIdWithCountAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var workshop = await db.Workshops
            .AsNoTracking()
            .Include(item => item.Instructor)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (workshop is null) return null;

        var count = await db.Enrollments.CountAsync(
            enrollment => enrollment.WorkshopId == id && enrollment.Status == EnrollmentStatus.Active,
            cancellationToken);

        return new WorkshopWithCount(workshop, count);
    }

    public async Task<IReadOnlyList<WorkshopWithCount>> GetTopAsync(CancellationToken cancellationToken = default)
    {
        var items = await db.Workshops
            .AsNoTracking()
            .Include(workshop => workshop.Instructor)
            .ToListAsync(cancellationToken);

        var counts = await ActiveCountsAsync(cancellationToken);
        return items
            .Select(workshop => new WorkshopWithCount(workshop, counts.GetValueOrDefault(workshop.Id)))
            .OrderByDescending(item => item.ActiveEnrollments)
            .ThenBy(item => item.Workshop.StartAt)
            .ToArray();
    }

    public Task AddAsync(Workshop workshop, CancellationToken cancellationToken = default) =>
        db.Workshops.AddAsync(workshop, cancellationToken).AsTask();

    private async Task<Dictionary<Guid, int>> ActiveCountsAsync(CancellationToken cancellationToken) =>
        await db.Enrollments
            .Where(enrollment => enrollment.Status == EnrollmentStatus.Active)
            .GroupBy(enrollment => enrollment.WorkshopId)
            .Select(group => new { WorkshopId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.WorkshopId, item => item.Count, cancellationToken);
}
