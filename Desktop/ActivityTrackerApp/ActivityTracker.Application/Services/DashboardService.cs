using ActivityTracker.Application.DTOs;
using ActivityTracker.Application.Interfaces;

namespace ActivityTracker.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IActivityLogRepository _activityLogRepository;

    public DashboardService(IAuditLogRepository auditLogRepository, IActivityLogRepository activityLogRepository)
    {
        _auditLogRepository = auditLogRepository;
        _activityLogRepository = activityLogRepository;
    }

    public async Task<ActivitySummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var allAudit = await _auditLogRepository.SearchForExportAsync(new DTOs.AuditLogFilterDto { PageSize = 10000 }, cancellationToken);
        return new ActivitySummaryDto
        {
            TotalAuditLogs = allAudit.Count,
            TotalDeletes = allAudit.Count(x => x.Action == "Delete"),
            TotalActivityLogs = await _activityLogRepository.CountAsync(x => x.ActivityType != string.Empty, cancellationToken),
            FailedLoginAttempts = await _activityLogRepository.CountAsync(x => x.ActivityType == "FailedLogin", cancellationToken),
            SuccessfulLogins = await _activityLogRepository.CountAsync(x => x.ActivityType == "Login", cancellationToken)
        };
    }

    public async Task<IReadOnlyCollection<RecentActivityDto>> GetRecentActivitiesAsync(CancellationToken cancellationToken = default)
    {
        var logs = await _activityLogRepository.GetRecentAsync(12, cancellationToken);
        return logs.Select(x => new RecentActivityDto
        {
            Id = x.Id,
            UserId = x.UserId,
            ActivityType = x.ActivityType,
            Description = x.Description,
            Endpoint = x.Endpoint,
            StatusCode = x.StatusCode,
            TimestampUtc = x.TimestampUtc
        }).ToArray();
    }
}
