using Microsoft.EntityFrameworkCore;
using OSTA.API.Contracts.BomImports;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Imports;

public class BomImportProcessingService
{
    private const string DefaultImportBaseUom = "EA";
    private const string DefaultImportPlantCode = "IMPORT";

    private readonly OstaDbContext _context;

    public BomImportProcessingService(OstaDbContext context)
    {
        _context = context;
    }

    public async Task<BOMImportBatch> ProcessAsync(CreateBomImportBatchRequestDto request)
    {
        var requestLinesByRowNumber = request.Lines.ToDictionary(x => x.RowNumber);

        var batch = new BOMImportBatch
        {
            Id = Guid.NewGuid(),
            SourceFileName = request.SourceFileName.Trim(),
            ImportedAtUtc = DateTimeOffset.UtcNow,
            Status = BOMImportBatchStatus.Pending,
            TotalRows = request.Lines.Count,
            SuccessfulRows = 0,
            FailedRows = 0,
            Lines = request.Lines.Select(line => new BOMImportLine
            {
                Id = Guid.NewGuid(),
                RowNumber = line.RowNumber,
                ProjectCode = line.ProjectCode.Trim(),
                ProjectName = line.ProjectName.Trim(),
                FinishedGoodCode = line.FinishedGoodCode.Trim(),
                FinishedGoodName = line.FinishedGoodName.Trim(),
                AssemblyCode = line.AssemblyCode.Trim(),
                AssemblyName = line.AssemblyName.Trim(),
                PartNumber = line.PartNumber.Trim(),
                Revision = line.Revision.Trim(),
                Description = line.Description.Trim(),
                Quantity = line.Quantity,
                MaterialCode = NormalizeOptional(line.MaterialCode),
                ThicknessMm = line.ThicknessMm,
                WeightKg = line.WeightKg,
                Notes = NormalizeOptional(line.Notes),
                ProcessRouteCode = NormalizeOptional(line.ProcessRouteCode),
                CutOnly = line.CutOnly,
                Status = BOMImportLineStatus.Pending
            }).ToList()
        };

        _context.BOMImportBatches.Add(batch);
        await _context.SaveChangesAsync();

        batch.Status = BOMImportBatchStatus.Processing;
        await _context.SaveChangesAsync();

        foreach (var line in batch.Lines.OrderBy(x => x.RowNumber))
        {
            var validationError = ValidateLine(line);
            if (validationError is not null)
            {
                line.Status = BOMImportLineStatus.Failed;
                line.ErrorMessage = validationError;
                batch.FailedRows++;
                await _context.SaveChangesAsync();
                continue;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                ProductDefinitionTraceability? traceability = null;

                if (request.ImportMode == BomImportMode.ExecutionAndProductDefinition)
                {
                    traceability = await ImportProductDefinitionAsync(line, requestLinesByRowNumber[line.RowNumber]);
                }

                var executionError = await ImportExecutionAsync(line, traceability);
                if (executionError is not null)
                {
                    line.Status = BOMImportLineStatus.Failed;
                    line.ErrorMessage = executionError;
                    batch.FailedRows++;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    continue;
                }

                line.Status = BOMImportLineStatus.Imported;
                line.ErrorMessage = null;
                batch.SuccessfulRows++;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                line.Status = BOMImportLineStatus.Failed;
                line.ErrorMessage = $"Unexpected import error: {ex.Message}";
                batch.FailedRows++;

                await _context.SaveChangesAsync();
            }
        }

        batch.Status = batch.SuccessfulRows > 0
            ? BOMImportBatchStatus.Completed
            : BOMImportBatchStatus.Failed;

        await _context.SaveChangesAsync();
        return batch;
    }

    private async Task<ProductDefinitionTraceability> ImportProductDefinitionAsync(BOMImportLine line, CreateBomImportLineRequestDto sourceLine)
    {
        var finishedGoodItem = await FindOrCreateOrUpdateItemMasterAsync(
            line.FinishedGoodCode,
            line.FinishedGoodName,
            line.FinishedGoodName,
            ItemType.FinishedGood,
            ProcurementType.Make,
            DefaultImportBaseUom,
            line.Revision);

        var assemblyItem = await FindOrCreateOrUpdateItemMasterAsync(
            line.AssemblyCode,
            line.AssemblyName,
            line.AssemblyName,
            ItemType.Assembly,
            ProcurementType.Make,
            DefaultImportBaseUom,
            line.Revision);

        var partItem = await FindOrCreateOrUpdateItemMasterAsync(
            line.PartNumber,
            line.Description,
            line.Description,
            ItemType.Part,
            ProcurementType.Make,
            DefaultImportBaseUom,
            line.Revision,
            item =>
            {
                item.MaterialCode = line.MaterialCode;
                item.ThicknessMm = line.ThicknessMm;
                item.WeightKg = line.WeightKg;
                item.DrawingNumber = NormalizeOptional(sourceLine.DrawingNumber);
                item.FinishCode = NormalizeOptional(sourceLine.FinishCode);
                item.Specification = NormalizeOptional(sourceLine.Specification);
                item.Notes = line.Notes;
            });

        var bomHeader = await FindOrCreateBomHeaderAsync(finishedGoodItem.Id, line.Revision);
        var bomItem = await FindOrCreateOrUpdateBomItemAsync(bomHeader, assemblyItem, line, sourceLine);

        return new ProductDefinitionTraceability(
            finishedGoodItem.Id,
            bomHeader.Id,
            bomItem.Id,
            assemblyItem.Id,
            partItem.Id);
    }

    private async Task<string?> ImportExecutionAsync(BOMImportLine line, ProductDefinitionTraceability? traceability)
    {
        var project = await FindOrCreateProjectAsync(line);
        var finishedGood = await FindOrCreateFinishedGoodAsync(project, line, traceability);
        var assembly = await FindOrCreateAssemblyAsync(finishedGood, line, traceability);

        var duplicatePartExists = await _context.Parts.AnyAsync(x =>
            x.AssemblyId == assembly.Id &&
            x.PartNumber == line.PartNumber &&
            x.Revision == line.Revision);

        if (duplicatePartExists)
        {
            return $"Part '{line.PartNumber}' revision '{line.Revision}' already exists in assembly '{line.AssemblyCode}'.";
        }

        _context.Parts.Add(new Part
        {
            Id = Guid.NewGuid(),
            PartNumber = line.PartNumber,
            Revision = line.Revision,
            Description = line.Description,
            AssemblyId = assembly.Id,
            SourceItemMasterId = traceability?.PartItemMasterId
        });

        await _context.SaveChangesAsync();
        return null;
    }

    private async Task<Project> FindOrCreateProjectAsync(BOMImportLine line)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Code == line.ProjectCode);

        if (project is not null)
        {
            return project;
        }

        project = new Project
        {
            Id = Guid.NewGuid(),
            Code = line.ProjectCode,
            Name = line.ProjectName
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return project;
    }

    private async Task<FinishedGood> FindOrCreateFinishedGoodAsync(
        Project project,
        BOMImportLine line,
        ProductDefinitionTraceability? traceability)
    {
        var finishedGood = await _context.FinishedGoods
            .FirstOrDefaultAsync(x => x.ProjectId == project.Id && x.Code == line.FinishedGoodCode);

        if (finishedGood is not null)
        {
            if (traceability is not null)
            {
                finishedGood.SourceItemMasterId = traceability.FinishedGoodItemMasterId;
                finishedGood.SourceBomHeaderId = traceability.BomHeaderId;
                await _context.SaveChangesAsync();
            }

            return finishedGood;
        }

        finishedGood = new FinishedGood
        {
            Id = Guid.NewGuid(),
            Code = line.FinishedGoodCode,
            Name = line.FinishedGoodName,
            ProjectId = project.Id,
            SourceItemMasterId = traceability?.FinishedGoodItemMasterId,
            SourceBomHeaderId = traceability?.BomHeaderId
        };

        _context.FinishedGoods.Add(finishedGood);
        await _context.SaveChangesAsync();

        return finishedGood;
    }

    private async Task<Assembly> FindOrCreateAssemblyAsync(
        FinishedGood finishedGood,
        BOMImportLine line,
        ProductDefinitionTraceability? traceability)
    {
        var assembly = await _context.Assemblies
            .FirstOrDefaultAsync(x => x.FinishedGoodId == finishedGood.Id && x.Code == line.AssemblyCode);

        if (assembly is not null)
        {
            if (traceability is not null)
            {
                assembly.SourceBomItemId = traceability.BomItemId;
                assembly.SourceComponentItemMasterId = traceability.AssemblyItemMasterId;
                await _context.SaveChangesAsync();
            }

            return assembly;
        }

        assembly = new Assembly
        {
            Id = Guid.NewGuid(),
            Code = line.AssemblyCode,
            Name = line.AssemblyName,
            FinishedGoodId = finishedGood.Id,
            SourceBomItemId = traceability?.BomItemId,
            SourceComponentItemMasterId = traceability?.AssemblyItemMasterId
        };

        _context.Assemblies.Add(assembly);
        await _context.SaveChangesAsync();

        return assembly;
    }

    private async Task<ItemMaster> FindOrCreateOrUpdateItemMasterAsync(
        string code,
        string name,
        string description,
        ItemType itemType,
        ProcurementType procurementType,
        string baseUom,
        string revision,
        Action<ItemMaster>? applyEnrichment = null)
    {
        var itemMaster = await _context.ItemMasters.FirstOrDefaultAsync(x => x.Code == code);

        if (itemMaster is null)
        {
            itemMaster = new ItemMaster
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = name,
                Description = description,
                ItemType = itemType,
                ProcurementType = procurementType,
                BaseUom = baseUom,
                Revision = revision,
                IsActive = true
            };

            applyEnrichment?.Invoke(itemMaster);
            _context.ItemMasters.Add(itemMaster);
            await _context.SaveChangesAsync();
            return itemMaster;
        }

        itemMaster.Name = name;
        itemMaster.Description = description;
        itemMaster.ItemType = itemType;
        itemMaster.ProcurementType = procurementType;
        itemMaster.BaseUom = string.IsNullOrWhiteSpace(itemMaster.BaseUom) ? baseUom : itemMaster.BaseUom;
        itemMaster.Revision = revision;
        itemMaster.IsActive = true;
        applyEnrichment?.Invoke(itemMaster);

        await _context.SaveChangesAsync();
        return itemMaster;
    }

    private async Task<BomHeader> FindOrCreateBomHeaderAsync(Guid parentItemMasterId, string revision)
    {
        var bomHeader = await _context.BomHeaders.FirstOrDefaultAsync(x =>
            x.ParentItemMasterId == parentItemMasterId &&
            x.Revision == revision &&
            x.Usage == BomUsage.Production &&
            x.PlantCode == DefaultImportPlantCode);

        if (bomHeader is not null)
        {
            if (bomHeader.Status != BomHeaderStatus.Active || bomHeader.BaseQuantity != 1)
            {
                bomHeader.Status = BomHeaderStatus.Active;
                bomHeader.BaseQuantity = 1;
                await _context.SaveChangesAsync();
            }

            return bomHeader;
        }

        bomHeader = new BomHeader
        {
            Id = Guid.NewGuid(),
            ParentItemMasterId = parentItemMasterId,
            Revision = revision,
            BaseQuantity = 1,
            Usage = BomUsage.Production,
            Status = BomHeaderStatus.Active,
            PlantCode = DefaultImportPlantCode
        };

        _context.BomHeaders.Add(bomHeader);
        await _context.SaveChangesAsync();

        return bomHeader;
    }

    private async Task<BomItem> FindOrCreateOrUpdateBomItemAsync(
        BomHeader bomHeader,
        ItemMaster componentItemMaster,
        BOMImportLine line,
        CreateBomImportLineRequestDto sourceLine)
    {
        var bomItem = await _context.BomItems.FirstOrDefaultAsync(x =>
            x.BomHeaderId == bomHeader.Id &&
            x.ComponentItemMasterId == componentItemMaster.Id);

        if (bomItem is null)
        {
            bomItem = new BomItem
            {
                Id = Guid.NewGuid(),
                BomHeaderId = bomHeader.Id,
                ItemNumber = await GenerateBomItemNumberAsync(bomHeader.Id, line.RowNumber),
                ComponentItemMasterId = componentItemMaster.Id,
                Uom = DefaultImportBaseUom
            };

            _context.BomItems.Add(bomItem);
        }

        bomItem.Quantity = line.Quantity;
        bomItem.Uom = DefaultImportBaseUom;
        bomItem.ItemCategory = BomItemCategory.Assembly;
        bomItem.ProcurementType = ProcurementType.Make;
        bomItem.ScrapPercent = sourceLine.ScrapPercent;
        bomItem.ProcessRouteCode = line.ProcessRouteCode;
        bomItem.LineNotes = line.Notes;
        bomItem.CutOnly = line.CutOnly;
        bomItem.SortOrder = line.RowNumber;

        await _context.SaveChangesAsync();
        return bomItem;
    }

    private async Task<string> GenerateBomItemNumberAsync(Guid bomHeaderId, int rowNumber)
    {
        var preferred = rowNumber.ToString("D4");
        var preferredTaken = await _context.BomItems.AnyAsync(x =>
            x.BomHeaderId == bomHeaderId &&
            x.ItemNumber == preferred);

        if (!preferredTaken)
        {
            return preferred;
        }

        var existingItemNumbers = await _context.BomItems
            .Where(x => x.BomHeaderId == bomHeaderId)
            .Select(x => x.ItemNumber)
            .ToListAsync();

        var nextNumeric = existingItemNumbers
            .Select(itemNumber => int.TryParse(itemNumber, out var value) ? value : 0)
            .DefaultIfEmpty(0)
            .Max();

        return (nextNumeric + 10).ToString("D4");
    }

    private static string? ValidateLine(BOMImportLine line)
    {
        if (string.IsNullOrWhiteSpace(line.ProjectCode)) return "ProjectCode is required.";
        if (string.IsNullOrWhiteSpace(line.ProjectName)) return "ProjectName is required.";
        if (string.IsNullOrWhiteSpace(line.FinishedGoodCode)) return "FinishedGoodCode is required.";
        if (string.IsNullOrWhiteSpace(line.FinishedGoodName)) return "FinishedGoodName is required.";
        if (string.IsNullOrWhiteSpace(line.AssemblyCode)) return "AssemblyCode is required.";
        if (string.IsNullOrWhiteSpace(line.AssemblyName)) return "AssemblyName is required.";
        if (string.IsNullOrWhiteSpace(line.PartNumber)) return "PartNumber is required.";
        if (string.IsNullOrWhiteSpace(line.Revision)) return "Revision is required.";
        if (string.IsNullOrWhiteSpace(line.Description)) return "Description is required.";
        if (line.Quantity <= 0) return "Quantity must be greater than zero.";
        return null;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record ProductDefinitionTraceability(
        Guid FinishedGoodItemMasterId,
        Guid BomHeaderId,
        Guid BomItemId,
        Guid AssemblyItemMasterId,
        Guid PartItemMasterId);
}
