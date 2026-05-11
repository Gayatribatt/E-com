using ActivityTracker.Application.DTOs;
using ActivityTracker.Application.Interfaces;
using ActivityTracker.Domain.Models;
using ActivityTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DynamicLinq = System.Linq.Dynamic.Core.DynamicQueryableExtensions;

namespace ActivityTracker.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _dbContext;

    public AuditLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ActivityTracker.Application.DTOs.PagedResult<AuditLog>> SearchAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(filter);
        var totalCount = await query.CountAsync(cancellationToken);
        var sortBy = string.IsNullOrWhiteSpace(filter.SortBy) ? "TimestampUtc" : filter.SortBy;
        var sortDirection = filter.Descending ? "descending" : "ascending";
        var ordered = DynamicLinq.OrderBy(query, $"{sortBy} {sortDirection}");
        var items = await ordered
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new ActivityTracker.Application.DTOs.PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<IReadOnlyCollection<AuditLog>> SearchForExportAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default)
    {
        return await BuildQuery(filter).OrderByDescending(x => x.TimestampUtc).AsNoTracking().ToListAsync(cancellationToken);
    }

    private IQueryable<AuditLog> BuildQuery(AuditLogFilterDto filter)
    {
        var query = _dbContext.AuditLogs.AsQueryable();
        if (filter.StartDateUtc.HasValue) query = query.Where(x => x.TimestampUtc >= filter.StartDateUtc.Value);
        if (filter.EndDateUtc.HasValue) query = query.Where(x => x.TimestampUtc <= filter.EndDateUtc.Value);
        if (filter.UserId.HasValue) query = query.Where(x => x.UserId == filter.UserId);
        if (!string.IsNullOrWhiteSpace(filter.Action)) query = query.Where(x => x.Action == filter.Action);
        if (!string.IsNullOrWhiteSpace(filter.EntityName)) query = query.Where(x => x.EntityName == filter.EntityName);
        return query;
    }
}
