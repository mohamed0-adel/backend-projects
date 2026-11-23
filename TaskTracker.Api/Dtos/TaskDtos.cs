using System.ComponentModel.DataAnnotations;
using TaskTracker.Api.Models;

namespace TaskTracker.Api.Dtos;

public class CreateTaskDto
{
    [Required, StringLength(100, MinimumLength = 3)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Title cannot contain only whitespace.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }
}

public class UpdateTaskDto : CreateTaskDto
{
    [EnumDataType(typeof(WorkStatus))]
    public WorkStatus Status { get; set; } = WorkStatus.Todo;
}

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public static TaskDto From(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        DueDate = task.DueDate,
        CreatedAt = task.CreatedAt
    };
}

public class PagedResult<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
