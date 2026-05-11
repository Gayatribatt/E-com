namespace ActivityTracker.Domain.Models;

public class ActivityLog : BaseEntity
{
    public int? UserId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string Method { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
