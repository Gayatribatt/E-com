namespace ActivityTracker.Application.DTOs;

public class RecentActivityDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public DateTime TimestampUtc { get; set; }
}
