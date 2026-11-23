using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ShortLink> Links => Set<ShortLink>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortLink>(entity =>
        {
            entity.ToTable(table => table.HasCheckConstraint(
                "CK_Links_Clicks_NonNegative", "[Clicks] >= 0"));

            // Codes are case-sensitive even when the SQL Server database uses a CI collation.
            entity.Property(link => link.Code)
                .HasMaxLength(20)
                .UseCollation("Latin1_General_100_BIN2")
                .IsRequired();

            entity.Property(link => link.OriginalUrl)
                .HasMaxLength(2048)
                .IsRequired();

            entity.Property(link => link.Clicks)
                .HasDefaultValue(0L);

            entity.Property(link => link.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(link => link.Code).IsUnique();
        });
    }
}
