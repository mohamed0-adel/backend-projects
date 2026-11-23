using TaskTracker.Api.Data;
using TaskTracker.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskTracker.Api.Migrations;

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

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.Property(task => task.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

            entity.Property(task => task.CreatedAt)
                .ValueGeneratedOnAdd()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(task => task.Description)
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            entity.Property(task => task.DueDate)
                .HasColumnType("datetime2");

            entity.Property(task => task.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)")
                .IsRequired();

            entity.Property(task => task.Title)
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)")
                .IsRequired();

            entity.HasKey(task => task.Id);
            entity.HasIndex(task => task.CreatedAt);
            entity.HasIndex(task => new { task.Status, task.CreatedAt });
            entity.ToTable("Tasks");
        });
    
    }
}
