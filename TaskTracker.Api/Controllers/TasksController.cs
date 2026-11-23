using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Dtos;
using TaskTracker.Api.Models;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(AppDbContext db) : ControllerBase
{
    /// <summary>List tasks with optional status filter, title search and paging.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetAll(
        [FromQuery] WorkStatus? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        IQueryable<TaskItem> query = db.Tasks.AsNoTracking();

        if (status.HasValue)
            query = query.Where(task => task.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(task => task.Title.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var offset = (long)(page - 1) * pageSize;
        var tasks = offset >= totalCount
            ? new List<TaskItem>()
            : await query
                .OrderByDescending(task => task.CreatedAt)
                .ThenByDescending(task => task.Id)
                .Skip((int)offset)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

        return Ok(new PagedResult<TaskDto>
        {
            Items = tasks.Select(TaskDto.From).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return task is null ? NotFound() : Ok(TaskDto.From(task));
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> Create(
        CreateTaskDto dto,
        CancellationToken cancellationToken)
    {
        var task = new TaskItem
        {
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            DueDate = dto.DueDate
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, TaskDto.From(task));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> Update(
        int id,
        UpdateTaskDto dto,
        CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FindAsync(new object[] { id }, cancellationToken);
        if (task is null)
            return NotFound();

        task.Title = dto.Title.Trim();
        task.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        task.DueDate = dto.DueDate;
        task.Status = dto.Status;

        await db.SaveChangesAsync(cancellationToken);
        return Ok(TaskDto.From(task));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FindAsync(new object[] { id }, cancellationToken);
        if (task is null)
            return NotFound();

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>Task counts grouped by status.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> Stats(CancellationToken cancellationToken)
    {
        var stats = await db.Tasks
            .AsNoTracking()
            .GroupBy(task => task.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .OrderBy(item => item.Status)
            .ToListAsync(cancellationToken);

        return Ok(stats);
    }
}
