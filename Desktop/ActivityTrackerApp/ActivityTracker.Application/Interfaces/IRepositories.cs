using System.Linq.Expressions;
using ActivityTracker.Application.DTOs;
using ActivityTracker.Domain.Models;

namespace ActivityTracker.Application.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task AddAsync(AppUser user, CancellationToken cancellationToken = default);
}

public interface ITaskRepository
{
    Task<IReadOnlyCollection<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(TaskItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(TaskItem item, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(TaskItem item, CancellationToken cancellationToken = default);
}

public interface IAuditLogRepository
{
    Task<PagedResult<AuditLog>> SearchAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AuditLog>> SearchForExportAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default);
}

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<ActivityLog, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ActivityLog>> GetRecentAsync(int take = 12, CancellationToken cancellationToken = default);
}
