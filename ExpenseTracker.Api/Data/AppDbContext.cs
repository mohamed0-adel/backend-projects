using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Expense> Expenses => Set<Expense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(user => user.Email)
                .HasMaxLength(254)
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(user => user.Email).IsUnique();
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_Expenses_Amount_Positive", "[Amount] > 0"));

            entity.Property(expense => expense.Description)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(expense => expense.Category)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(expense => expense.Amount)
                .HasPrecision(18, 2);

            entity.HasOne(expense => expense.User)
                .WithMany(user => user.Expenses)
                .HasForeignKey(expense => expense.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(expense => new { expense.UserId, expense.Date });
            entity.HasIndex(expense => new { expense.UserId, expense.Category, expense.Date });
        });
    }
}
