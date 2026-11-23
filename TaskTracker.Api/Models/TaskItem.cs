namespace TaskTracker.Api.Models;

public enum WorkStatus
{
    Todo,
    InProgress,
    Done
}

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkStatus Status { get; set; } = WorkStatus.Todo;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
