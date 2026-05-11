using ActivityTracker.Application.DTOs;
using ActivityTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActivityTracker.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/audit-logs")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AuditLogResponseDto>>> Search([FromQuery] AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        return Ok(await _auditService.SearchAsync(filter, cancellationToken));
    }

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] AuditLogFilterDto filter, CancellationToken cancellationToken)
    {
        var content = await _auditService.ExportCsvAsync(filter, cancellationToken);
        return File(System.Text.Encoding.UTF8.GetBytes(content), "text/csv", $"audit-logs-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }
}
