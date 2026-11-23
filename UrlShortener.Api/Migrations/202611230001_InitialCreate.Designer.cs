using UrlShortener.Api.Data;
using UrlShortener.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Api.Migrations;

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

        modelBuilder.Entity<ShortLink>(entity =>
        {
            entity.Property(link => link.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("int");

            entity.Property(link => link.Code)
                .UseCollation("Latin1_General_100_BIN2")
                .HasMaxLength(20)
                .HasColumnType("nvarchar(20)")
                .IsRequired();

            entity.Property(link => link.Clicks)
                .HasColumnType("bigint")
                .HasDefaultValue(0L);

            entity.Property(link => link.CreatedAt)
                .ValueGeneratedOnAdd()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(link => link.ExpiresAt)
                .HasColumnType("datetime2");

            entity.Property(link => link.OriginalUrl)
                .HasMaxLength(2048)
                .HasColumnType("nvarchar(2048)")
                .IsRequired();

            entity.HasKey(link => link.Id);
            entity.HasIndex(link => link.Code).IsUnique();
            entity.ToTable("Links", table =>
                table.HasCheckConstraint("CK_Links_Clicks_NonNegative", "[Clicks] >= 0"));
        });
    
    }
}
