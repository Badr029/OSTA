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
    public DbSet<BOMImportBatch> BOMImportBatches => Set<BOMImportBatch>();
    public DbSet<BOMImportLine> BOMImportLines => Set<BOMImportLine>();

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

        modelBuilder.Entity<BOMImportBatch>(entity =>
        {
            entity.ToTable("bom_import_batches");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.SourceFileName)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(x => x.ImportedAtUtc)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.TotalRows)
                .IsRequired();

            entity.Property(x => x.SuccessfulRows)
                .IsRequired();

            entity.Property(x => x.FailedRows)
                .IsRequired();
        });

        modelBuilder.Entity<BOMImportLine>(entity =>
        {
            entity.ToTable("bom_import_lines");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.BOMImportBatchId)
                .IsRequired();

            entity.Property(x => x.RowNumber)
                .IsRequired();

            entity.Property(x => x.ProjectCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ProjectName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.FinishedGoodCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.FinishedGoodName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.AssemblyCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.AssemblyName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.PartNumber)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Revision)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.Quantity)
                .HasPrecision(18, 4)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.ErrorMessage)
                .HasMaxLength(1000);

            entity.HasOne(x => x.BOMImportBatch)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.BOMImportBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.BOMImportBatchId, x.RowNumber })
                .IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}
