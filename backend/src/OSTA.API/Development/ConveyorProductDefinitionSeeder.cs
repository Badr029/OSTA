using Microsoft.EntityFrameworkCore;
using OSTA.Domain.Entities;
using OSTA.Infrastructure.Persistence;

namespace OSTA.API.Development;

public class ConveyorProductDefinitionSeeder
{
    private const string ParentCode = "MCB-0600X0075";
    private const string Revision = "A";
    private const string PlantCode = "SMALL-FACTORY-01";
    private const string Usage = "Production";
    private readonly OstaDbContext _context;

    public ConveyorProductDefinitionSeeder(OstaDbContext context)
    {
        _context = context;
    }

    public async Task<ConveyorSeedResultDto> SeedAsync(CancellationToken cancellationToken = default)
    {
        var definitions = GetItemDefinitions();
        var itemsByCode = await _context.ItemMasters
            .Where(x => definitions.Select(d => d.Code).Contains(x.Code))
            .ToDictionaryAsync(x => x.Code, cancellationToken);

        foreach (var definition in definitions)
        {
            if (!itemsByCode.TryGetValue(definition.Code, out var item))
            {
                item = new ItemMaster
                {
                    Id = Guid.NewGuid(),
                    Code = definition.Code,
                    Name = definition.Name,
                    Description = definition.Description,
                    ItemType = definition.ItemType,
                    ProcurementType = definition.ProcurementType,
                    BaseUom = "EA",
                    Revision = Revision,
                    IsActive = true
                };

                _context.ItemMasters.Add(item);
                itemsByCode.Add(item.Code, item);
            }
            else
            {
                item.Name = definition.Name;
                item.Description = definition.Description;
                item.ItemType = definition.ItemType;
                item.ProcurementType = definition.ProcurementType;
                item.BaseUom = "EA";
                item.Revision = Revision;
                item.IsActive = true;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var parentItem = itemsByCode[ParentCode];
        var bomHeader = await _context.BomHeaders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x =>
                x.ParentItemMasterId == parentItem.Id &&
                x.Revision == Revision &&
                x.Usage == BomUsage.Production &&
                x.PlantCode == PlantCode,
                cancellationToken);

        if (bomHeader is null)
        {
            bomHeader = new BomHeader
            {
                Id = Guid.NewGuid(),
                ParentItemMasterId = parentItem.Id,
                Revision = Revision,
                BaseQuantity = 1,
                Usage = BomUsage.Production,
                Status = BomHeaderStatus.Active,
                PlantCode = PlantCode
            };

            _context.BomHeaders.Add(bomHeader);
            await _context.SaveChangesAsync(cancellationToken);
            await _context.Entry(bomHeader).Collection(x => x.Items).LoadAsync(cancellationToken);
        }
        else
        {
            bomHeader.BaseQuantity = 1;
            bomHeader.Usage = BomUsage.Production;
            bomHeader.Status = BomHeaderStatus.Active;
            bomHeader.PlantCode = PlantCode;
        }

        var lineDefinitions = GetBomLineDefinitions(itemsByCode);
        var linesByItemNumber = bomHeader.Items.ToDictionary(x => x.ItemNumber);

        foreach (var line in lineDefinitions)
        {
            if (!linesByItemNumber.TryGetValue(line.ItemNumber, out var bomItem))
            {
                bomItem = new BomItem
                {
                    Id = Guid.NewGuid(),
                    BomHeaderId = bomHeader.Id,
                    ItemNumber = line.ItemNumber,
                    ComponentItemMasterId = line.ComponentItemMasterId,
                    Quantity = line.Quantity,
                    Uom = line.Uom,
                    ItemCategory = line.ItemCategory,
                    ProcurementType = line.ProcurementType,
                    SortOrder = line.SortOrder
                };

                _context.BomItems.Add(bomItem);
            }
            else
            {
                bomItem.ComponentItemMasterId = line.ComponentItemMasterId;
                bomItem.Quantity = line.Quantity;
                bomItem.Uom = line.Uom;
                bomItem.ItemCategory = line.ItemCategory;
                bomItem.ProcurementType = line.ProcurementType;
                bomItem.SortOrder = line.SortOrder;
            }
        }

        var allowedItemNumbers = lineDefinitions.Select(x => x.ItemNumber).ToHashSet();
        var extraLines = bomHeader.Items.Where(x => !allowedItemNumbers.Contains(x.ItemNumber)).ToList();
        if (extraLines.Count > 0)
        {
            _context.BomItems.RemoveRange(extraLines);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ConveyorSeedResultDto(
            parentItem.Id,
            parentItem.Code,
            bomHeader.Id,
            lineDefinitions.Count
        );
    }

    private static List<ConveyorItemDefinition> GetItemDefinitions()
    {
        return
        [
            new(
                ParentCode,
                "Modular Belt Conveyor 600x75",
                "Finished good modular belt conveyor for small factory reference product",
                ItemType.FinishedGood,
                ProcurementType.Make),
            new(
                "MCB-0600X0075-DRIVE",
                "Drive Unit Assembly",
                "Drive module assembly for modular belt conveyor",
                ItemType.Assembly,
                ProcurementType.Make),
            new(
                "MCB-0600X0075-FRAME",
                "Frame Fabrication Assembly",
                "Fabricated frame assembly for modular belt conveyor",
                ItemType.Assembly,
                ProcurementType.Make),
            new(
                "MCB-0600X0075-TAIL",
                "Tail Assembly",
                "Tail end assembly for modular belt conveyor",
                ItemType.Assembly,
                ProcurementType.Make),
            new(
                "MCB-0600X0075-BELT",
                "Belt Assembly",
                "Modular belt assembly for conveyor product definition",
                ItemType.Assembly,
                ProcurementType.Make),
            new(
                "MCB-0600X0075-SUPPORT",
                "Support Assembly",
                "Support assembly for modular belt conveyor",
                ItemType.Assembly,
                ProcurementType.Make),
            new(
                "MCB-0600X0075-GUIDE",
                "Guide Assembly",
                "Guide assembly for modular belt conveyor",
                ItemType.Assembly,
                ProcurementType.Make),
            new(
                "MCB-0600X0075-HW",
                "Hardware Kit",
                "Hardware kit for modular belt conveyor",
                ItemType.HardwareKit,
                ProcurementType.Buy)
        ];
    }

    private static List<ConveyorBomLineDefinition> GetBomLineDefinitions(IReadOnlyDictionary<string, ItemMaster> itemsByCode)
    {
        return
        [
            new("0010", itemsByCode["MCB-0600X0075-DRIVE"].Id, 1, "EA", BomItemCategory.Assembly, ProcurementType.Make, 10),
            new("0020", itemsByCode["MCB-0600X0075-FRAME"].Id, 1, "EA", BomItemCategory.Assembly, ProcurementType.Make, 20),
            new("0030", itemsByCode["MCB-0600X0075-TAIL"].Id, 1, "EA", BomItemCategory.Assembly, ProcurementType.Make, 30),
            new("0040", itemsByCode["MCB-0600X0075-BELT"].Id, 1, "EA", BomItemCategory.Assembly, ProcurementType.Make, 40),
            new("0050", itemsByCode["MCB-0600X0075-SUPPORT"].Id, 1, "EA", BomItemCategory.Assembly, ProcurementType.Make, 50),
            new("0060", itemsByCode["MCB-0600X0075-GUIDE"].Id, 1, "EA", BomItemCategory.Assembly, ProcurementType.Make, 60),
            new("0070", itemsByCode["MCB-0600X0075-HW"].Id, 1, "EA", BomItemCategory.Kit, ProcurementType.Buy, 70)
        ];
    }

    public sealed record ConveyorSeedResultDto(
        Guid ParentItemMasterId,
        string ParentItemCode,
        Guid BomHeaderId,
        int BomItemCount
    );

    private sealed record ConveyorItemDefinition(
        string Code,
        string Name,
        string Description,
        ItemType ItemType,
        ProcurementType ProcurementType
    );

    private sealed record ConveyorBomLineDefinition(
        string ItemNumber,
        Guid ComponentItemMasterId,
        decimal Quantity,
        string Uom,
        BomItemCategory ItemCategory,
        ProcurementType ProcurementType,
        int SortOrder
    );
}
