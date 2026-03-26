using Microsoft.EntityFrameworkCore;
using OSTA.Domain.Entities;

namespace OSTA.Infrastructure.Persistence;

public class OstaDbContext : DbContext
{
    public OstaDbContext(DbContextOptions<OstaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<FinishedGood> FinishedGoods => Set<FinishedGood>();
    public DbSet<Assembly> Assemblies => Set<Assembly>();
    public DbSet<Part> Parts => Set<Part>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("projects");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(x => x.Code)
                .IsUnique();
        });

        modelBuilder.Entity<FinishedGood>(entity =>
        {
            entity.ToTable("finished_goods");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.ProjectId)
                .IsRequired();

            entity.HasOne(x => x.Project)
                .WithMany(x => x.FinishedGoods)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.ProjectId, x.Code })
                .IsUnique();
        });

        modelBuilder.Entity<Assembly>(entity =>
        {
            entity.ToTable("assemblies");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.FinishedGoodId)
                .IsRequired();

            entity.HasOne(x => x.FinishedGood)
                .WithMany(x => x.Assemblies)
                .HasForeignKey(x => x.FinishedGoodId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.FinishedGoodId, x.Code })
                .IsUnique();
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.ToTable("parts");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PartNumber)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Revision)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.AssemblyId)
                .IsRequired();

            entity.HasOne(x => x.Assembly)
                .WithMany(x => x.Parts)
                .HasForeignKey(x => x.AssemblyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.AssemblyId, x.PartNumber, x.Revision })
                .IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
