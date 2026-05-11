using System.Linq.Expressions;
using ActivityTracker.Application.Interfaces;
using ActivityTracker.Domain.Models;
using ActivityTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ActivityTracker.Infrastructure.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly AppDbContext _dbContext;

    public ActivityLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default)
    {
        _dbContext.ActivityLogs.Add(log);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<int> CountAsync(Expression<Func<ActivityLog, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return _dbContext.ActivityLogs.CountAsync(predicate, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ActivityLog>> GetRecentAsync(int take = 12, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ActivityLogs
            .AsNoTracking()
            .OrderByDescending(x => x.TimestampUtc)
            .Take(take)
            .ToArrayAsync(cancellationToken);
    }
}
