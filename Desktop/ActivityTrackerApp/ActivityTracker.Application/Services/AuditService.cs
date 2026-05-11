using System.Text;
using System.Text.Json;
using ActivityTracker.Application.DTOs;
using ActivityTracker.Application.Interfaces;
using CsvHelper;
using System.Globalization;

namespace ActivityTracker.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedResult<AuditLogResponseDto>> SearchAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default)
    {
        var result = await _auditLogRepository.SearchAsync(filter, cancellationToken);
        return new PagedResult<AuditLogResponseDto>
        {
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            Items = result.Items.Select(x => new AuditLogResponseDto
            {
                Id = x.Id,
                UserId = x.UserId,
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                OldValuesJson = x.OldValuesJson,
                NewValuesJson = x.NewValuesJson,
                IpAddress = x.IpAddress,
                TimestampUtc = x.TimestampUtc,
                ChangedFields = ComputeChangedFields(x.OldValuesJson, x.NewValuesJson)
            }).ToArray()
        };
    }

    public async Task<string> ExportCsvAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.SearchForExportAsync(filter, cancellationToken);
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(logs.Select(x => new
        {
            x.Id,
            x.UserId,
            x.Action,
            x.EntityName,
            x.EntityId,
            x.OldValuesJson,
            x.NewValuesJson,
            x.IpAddress,
            x.TimestampUtc
        }));
        return writer.ToString();
    }

    private static List<ChangedFieldDto> ComputeChangedFields(string oldJson, string newJson)
    {
        var oldDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(oldJson) ?? [];
        var newDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(newJson) ?? [];
        var keys = oldDict.Keys.Union(newDict.Keys).Distinct();
        var changes = new List<ChangedFieldDto>();

        foreach (var key in keys)
        {
            var oldValue = oldDict.TryGetValue(key, out var ov) ? ov.ToString() : null;
            var newValue = newDict.TryGetValue(key, out var nv) ? nv.ToString() : null;
            if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
            {
                changes.Add(new ChangedFieldDto { Field = key, OldValue = oldValue, NewValue = newValue });
            }
        }
        return changes;
    }
}
