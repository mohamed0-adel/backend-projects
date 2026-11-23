using ExpenseTracker.Api.Data;
using ExpenseTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Api.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202611230001_InitialCreate")]
public partial class InitialCreate
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.15")
            .HasAnnotation("Relational:MaxIdentifierLength", 128)
            .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(user => user.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

            entity.Property(user => user.Email)
                .HasMaxLength(254)
                .HasColumnType("nvarchar(254)")
                .IsRequired();

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .IsRequired();

            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.ToTable("Users");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.Property(expense => expense.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

            entity.Property(expense => expense.Amount)
                .HasPrecision(18, 2)
                .HasColumnType("decimal(18,2)");

            entity.Property(expense => expense.Category)
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)")
                .IsRequired();

            entity.Property(expense => expense.Date)
                .HasColumnType("date");

            entity.Property(expense => expense.Description)
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)")
                .IsRequired();

            entity.Property(expense => expense.UserId)
                .HasColumnType("int");

            entity.HasKey(expense => expense.Id);
            entity.HasIndex(expense => new { expense.UserId, expense.Category, expense.Date });
            entity.HasIndex(expense => new { expense.UserId, expense.Date });

            entity.HasOne(expense => expense.User)
                .WithMany(user => user.Expenses)
                .HasForeignKey(expense => expense.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.ToTable("Expenses", table =>
                table.HasCheckConstraint("CK_Expenses_Amount_Positive", "[Amount] > 0"));
        });
    
    }
}
