namespace ActivityTracker.Domain.Models;

public class AppUser : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}
