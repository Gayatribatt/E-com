using ActivityTracker.Application.Interfaces;
using ActivityTracker.Domain.Models;
using ActivityTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ActivityTracker.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _dbContext;

    public TaskRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tasks.AsNoTracking().OrderByDescending(x => x.Id).ToArrayAsync(cancellationToken);
    }

    public Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Tasks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task AddAsync(TaskItem item, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskItem item, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Update(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SoftDeleteAsync(TaskItem item, CancellationToken cancellationToken = default)
    {
        item.IsDeleted = true;
        item.DeletedAtUtc = DateTime.UtcNow;
        _dbContext.Tasks.Update(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
