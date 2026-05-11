using ActivityTracker.Application.DTOs;

namespace ActivityTracker.Application.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
}

public interface ITaskService
{
    Task<IReadOnlyCollection<TaskResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TaskResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TaskResponseDto> CreateAsync(UpsertTaskDto request, CancellationToken cancellationToken = default);
    Task<TaskResponseDto> UpdateAsync(int id, UpsertTaskDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface IAuditService
{
    Task<PagedResult<AuditLogResponseDto>> SearchAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default);
    Task<string> ExportCsvAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default);
}

public interface IDashboardService
{
    Task<ActivitySummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<RecentActivityDto>> GetRecentActivitiesAsync(CancellationToken cancellationToken = default);
}
