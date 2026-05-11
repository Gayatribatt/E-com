namespace ActivityTracker.Domain.Models;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int CreatedByUserId { get; set; }
}
