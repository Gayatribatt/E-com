namespace ActivityTracker.Application.DTOs;

public class ActivitySummaryDto
{
    public int TotalAuditLogs { get; set; }
    public int TotalActivityLogs { get; set; }
    public int FailedLoginAttempts { get; set; }
    public int SuccessfulLogins { get; set; }
    public int TotalDeletes { get; set; }
}
