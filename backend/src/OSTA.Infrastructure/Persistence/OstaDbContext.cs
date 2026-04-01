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
    public DbSet<ItemMaster> ItemMasters => Set<ItemMaster>();
    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
    public DbSet<RoutingTemplate> RoutingTemplates => Set<RoutingTemplate>();
    public DbSet<RoutingOperation> RoutingOperations => Set<RoutingOperation>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderOperation> WorkOrderOperations => Set<WorkOrderOperation>();
    public DbSet<BomHeader> BomHeaders => Set<BomHeader>();
    public DbSet<BomItem> BomItems => Set<BomItem>();
    public DbSet<BomImportTemplate> BomImportTemplates => Set<BomImportTemplate>();
    public DbSet<BomImportTemplateFieldMapping> BomImportTemplateFieldMappings => Set<BomImportTemplateFieldMapping>();
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

            entity.Property(x => x.SourceItemMasterId);

            entity.Property(x => x.SourceBomHeaderId);

            entity.HasOne(x => x.Project)
                .WithMany(x => x.FinishedGoods)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.SourceItemMaster)
                .WithMany()
                .HasForeignKey(x => x.SourceItemMasterId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.SourceBomHeader)
                .WithMany()
                .HasForeignKey(x => x.SourceBomHeaderId)
                .OnDelete(DeleteBehavior.SetNull);

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

            entity.Property(x => x.SourceBomItemId);

            entity.Property(x => x.SourceComponentItemMasterId);

            entity.HasOne(x => x.FinishedGood)
                .WithMany(x => x.Assemblies)
                .HasForeignKey(x => x.FinishedGoodId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.SourceBomItem)
                .WithMany()
                .HasForeignKey(x => x.SourceBomItemId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.SourceComponentItemMaster)
                .WithMany()
                .HasForeignKey(x => x.SourceComponentItemMasterId)
                .OnDelete(DeleteBehavior.SetNull);

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

            entity.Property(x => x.SourceItemMasterId);

            entity.HasOne(x => x.Assembly)
                .WithMany(x => x.Parts)
                .HasForeignKey(x => x.AssemblyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.SourceItemMaster)
                .WithMany()
                .HasForeignKey(x => x.SourceItemMasterId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => new { x.AssemblyId, x.PartNumber, x.Revision })
                .IsUnique();
        });

        modelBuilder.Entity<ItemMaster>(entity =>
        {
            entity.ToTable("item_masters");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.MaterialCode)
                .HasMaxLength(100);

            entity.Property(x => x.ThicknessMm)
                .HasPrecision(18, 4);

            entity.Property(x => x.WeightKg)
                .HasPrecision(18, 4);

            entity.Property(x => x.LengthMm)
                .HasPrecision(18, 4);

            entity.Property(x => x.WidthMm)
                .HasPrecision(18, 4);

            entity.Property(x => x.HeightMm)
                .HasPrecision(18, 4);

            entity.Property(x => x.DrawingNumber)
                .HasMaxLength(100);

            entity.Property(x => x.FinishCode)
                .HasMaxLength(100);

            entity.Property(x => x.Specification)
                .HasMaxLength(500);

            entity.Property(x => x.Notes)
                .HasMaxLength(2000);

            entity.Property(x => x.ItemType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.ProcurementType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.BaseUom)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Revision)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.HasIndex(x => x.Code)
                .IsUnique();
        });

        modelBuilder.Entity<WorkCenter>(entity =>
        {
            entity.ToTable("work_centers");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Department)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.HourlyRate)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.HasIndex(x => x.Code)
                .IsUnique();
        });

        modelBuilder.Entity<RoutingTemplate>(entity =>
        {
            entity.ToTable("routing_templates");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ItemMasterId)
                .IsRequired();

            entity.Property(x => x.Code)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Revision)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.HasOne(x => x.ItemMaster)
                .WithMany(x => x.RoutingTemplates)
                .HasForeignKey(x => x.ItemMasterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.ItemMasterId, x.Code, x.Revision })
                .IsUnique();
        });

        modelBuilder.Entity<RoutingOperation>(entity =>
        {
            entity.ToTable("routing_operations");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.RoutingTemplateId)
                .IsRequired();

            entity.Property(x => x.OperationNumber)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.OperationCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.OperationName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.WorkCenterId)
                .IsRequired();

            entity.Property(x => x.SetupTimeMinutes)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(x => x.RunTimeMinutes)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(x => x.Sequence)
                .IsRequired();

            entity.Property(x => x.IsQcGate)
                .IsRequired();

            entity.HasOne(x => x.RoutingTemplate)
                .WithMany(x => x.Operations)
                .HasForeignKey(x => x.RoutingTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.WorkCenter)
                .WithMany(x => x.RoutingOperations)
                .HasForeignKey(x => x.WorkCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.RoutingTemplateId, x.OperationNumber })
                .IsUnique();
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.ToTable("work_orders");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ProjectId)
                .IsRequired();

            entity.Property(x => x.FinishedGoodId)
                .IsRequired();

            entity.Property(x => x.AssemblyId)
                .IsRequired();

            entity.Property(x => x.WorkOrderNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.PlannedQuantity)
                .HasPrecision(18, 4)
                .IsRequired();

            entity.Property(x => x.CompletedQuantity)
                .HasPrecision(18, 4)
                .IsRequired();

            entity.Property(x => x.ReleasedAtUtc);

            entity.Property(x => x.ClosedAtUtc);

            entity.HasOne(x => x.Project)
                .WithMany(x => x.WorkOrders)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.FinishedGood)
                .WithMany(x => x.WorkOrders)
                .HasForeignKey(x => x.FinishedGoodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Assembly)
                .WithMany(x => x.WorkOrders)
                .HasForeignKey(x => x.AssemblyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkOrderNumber)
                .IsUnique();

            entity.HasIndex(x => x.AssemblyId)
                .IsUnique();
        });

        modelBuilder.Entity<WorkOrderOperation>(entity =>
        {
            entity.ToTable("work_order_operations");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkOrderId)
                .IsRequired();

            entity.Property(x => x.RoutingOperationId);

            entity.Property(x => x.OperationNumber)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.OperationCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.OperationName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.WorkCenterId)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.PlannedQuantity)
                .HasPrecision(18, 4)
                .IsRequired();

            entity.Property(x => x.CompletedQuantity)
                .HasPrecision(18, 4)
                .IsRequired();

            entity.Property(x => x.Sequence)
                .IsRequired();

            entity.Property(x => x.IsQcGate)
                .IsRequired();

            entity.HasOne(x => x.WorkOrder)
                .WithMany(x => x.Operations)
                .HasForeignKey(x => x.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.RoutingOperation)
                .WithMany(x => x.WorkOrderOperations)
                .HasForeignKey(x => x.RoutingOperationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.WorkCenter)
                .WithMany(x => x.WorkOrderOperations)
                .HasForeignKey(x => x.WorkCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.WorkOrderId, x.OperationNumber })
                .IsUnique();
        });

        modelBuilder.Entity<BomHeader>(entity =>
        {
            entity.ToTable("bom_headers");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ParentItemMasterId)
                .IsRequired();

            entity.Property(x => x.Revision)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.BaseQuantity)
                .HasPrecision(18, 4)
                .IsRequired();

            entity.Property(x => x.Usage)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.PlantCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasOne(x => x.ParentItemMaster)
                .WithMany(x => x.BomHeaders)
                .HasForeignKey(x => x.ParentItemMasterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.ParentItemMasterId, x.Revision, x.Usage, x.PlantCode })
                .IsUnique();
        });

        modelBuilder.Entity<BomItem>(entity =>
        {
            entity.ToTable("bom_items");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.BomHeaderId)
                .IsRequired();

            entity.Property(x => x.ItemNumber)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.ComponentItemMasterId)
                .IsRequired();

            entity.Property(x => x.Quantity)
                .HasPrecision(18, 4)
                .IsRequired();

            entity.Property(x => x.Uom)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.ItemCategory)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.ProcurementType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.ScrapPercent)
                .HasPrecision(18, 4);

            entity.Property(x => x.ProcessRouteCode)
                .HasMaxLength(100);

            entity.Property(x => x.PositionText)
                .HasMaxLength(200);

            entity.Property(x => x.LineNotes)
                .HasMaxLength(2000);

            entity.Property(x => x.SortOrder)
                .IsRequired();

            entity.HasOne(x => x.BomHeader)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.BomHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ComponentItemMaster)
                .WithMany(x => x.ComponentBomItems)
                .HasForeignKey(x => x.ComponentItemMasterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.BomHeaderId, x.ItemNumber })
                .IsUnique();
        });

        modelBuilder.Entity<BomImportTemplate>(entity =>
        {
            entity.ToTable("bom_import_templates");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.FormatType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.StructureType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.HeaderRowIndex)
                .IsRequired();

            entity.Property(x => x.DataStartRowIndex)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.Property(x => x.Notes)
                .HasMaxLength(2000);

            entity.HasIndex(x => x.Code)
                .IsUnique();
        });

        modelBuilder.Entity<BomImportTemplateFieldMapping>(entity =>
        {
            entity.ToTable("bom_import_template_field_mappings");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.BomImportTemplateId)
                .IsRequired();

            entity.Property(x => x.TargetField)
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(x => x.SourceColumnName)
                .HasMaxLength(200);

            entity.Property(x => x.DefaultValue)
                .HasMaxLength(500);

            entity.Property(x => x.IsRequired)
                .IsRequired();

            entity.Property(x => x.SortOrder)
                .IsRequired();

            entity.HasOne(x => x.BomImportTemplate)
                .WithMany(x => x.FieldMappings)
                .HasForeignKey(x => x.BomImportTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.BomImportTemplateId, x.TargetField })
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

            entity.Property(x => x.MaterialCode)
                .HasMaxLength(100);

            entity.Property(x => x.ThicknessMm)
                .HasPrecision(18, 4);

            entity.Property(x => x.WeightKg)
                .HasPrecision(18, 4);

            entity.Property(x => x.Notes)
                .HasMaxLength(2000);

            entity.Property(x => x.ProcessRouteCode)
                .HasMaxLength(100);

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
