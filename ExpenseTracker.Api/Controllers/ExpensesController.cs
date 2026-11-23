using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Dtos;
using ExpenseTracker.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ExpensesController(AppDbContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    // Every expense query is scoped to the authenticated user.
    private IQueryable<Expense> Mine() => db.Expenses.Where(expense => expense.UserId == UserId);

    private IQueryable<Expense> Filtered(DateOnly? from, DateOnly? to, string? category)
    {
        var query = Mine().AsNoTracking();

        if (from.HasValue)
            query = query.Where(expense => expense.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(expense => expense.Date <= to.Value);

        if (!string.IsNullOrWhiteSpace(category))
        {
            var categoryFilter = category.Trim();
            query = query.Where(expense => expense.Category == categoryFilter);
        }

        return query;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetAll(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            return BadRequest(new ProblemDetails { Title = "The 'from' date must be on or before the 'to' date." });

        var expenses = await Filtered(from, to, category)
            .OrderByDescending(expense => expense.Date)
            .ThenByDescending(expense => expense.Id)
            .ToListAsync(cancellationToken);

        return Ok(expenses.Select(ExpenseDto.From));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var expense = await Mine().AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return expense is null ? NotFound() : Ok(ExpenseDto.From(expense));
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(
        ExpenseInputDto dto,
        CancellationToken cancellationToken)
    {
        var expense = new Expense
        {
            Description = dto.Description.Trim(),
            Amount = dto.Amount,
            Category = dto.Category.Trim(),
            Date = dto.Date,
            UserId = UserId
        };

        db.Expenses.Add(expense);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, ExpenseDto.From(expense));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExpenseDto>> Update(
        int id,
        ExpenseInputDto dto,
        CancellationToken cancellationToken)
    {
        var expense = await Mine().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (expense is null)
            return NotFound();

        expense.Description = dto.Description.Trim();
        expense.Amount = dto.Amount;
        expense.Category = dto.Category.Trim();
        expense.Date = dto.Date;

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ExpenseDto.From(expense));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var expense = await Mine().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (expense is null)
            return NotFound();

        db.Expenses.Remove(expense);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>Returns total spending and category totals for an optional date range.</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ExpenseSummary>> Summary(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            return BadRequest(new ProblemDetails { Title = "The 'from' date must be on or before the 'to' date." });

        var expenses = Filtered(from, to, null);

        // SQL Server performs the aggregation; expense rows are not loaded into application memory.
        var totals = await expenses
            .GroupBy(expense => expense.UserId)
            .Select(group => new
            {
                GrandTotal = group.Sum(expense => expense.Amount),
                Count = group.Count()
            })
            .SingleOrDefaultAsync(cancellationToken);

        var byCategory = await expenses
            .GroupBy(expense => expense.Category)
            .Select(group => new CategoryTotal(group.Key, group.Sum(expense => expense.Amount)))
            .OrderByDescending(item => item.Total)
            .ThenBy(item => item.Category)
            .ToListAsync(cancellationToken);

        return Ok(new ExpenseSummary(
            totals?.GrandTotal ?? 0m,
            totals?.Count ?? 0,
            byCategory));
    }
}
