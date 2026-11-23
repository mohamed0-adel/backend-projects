using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Api.Models;

namespace ExpenseTracker.Api.Dtos;

public class RegisterDto
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Password { get; set; } = string.Empty;
}

public record AuthResponse(string Token, string Email);

public class ExpenseInputDto
{
    [Required, StringLength(200, MinimumLength = 2)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Description cannot contain only whitespace.")]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Amount { get; set; }

    [Required, StringLength(50, MinimumLength = 1)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Category cannot contain only whitespace.")]
    public string Category { get; set; } = "Other";

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
}

public record ExpenseDto(int Id, string Description, decimal Amount, string Category, DateOnly Date)
{
    public static ExpenseDto From(Expense expense) => new(
        expense.Id,
        expense.Description,
        expense.Amount,
        expense.Category,
        expense.Date);
}

public record CategoryTotal(string Category, decimal Total);
public record ExpenseSummary(decimal GrandTotal, int Count, IReadOnlyList<CategoryTotal> ByCategory);
