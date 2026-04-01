namespace OSTA.Domain.Entities;

public class BomItem
{
    public Guid Id { get; set; }

    public Guid BomHeaderId { get; set; }
    public BomHeader BomHeader { get; set; } = null!;

    public required string ItemNumber { get; set; }

    public Guid ComponentItemMasterId { get; set; }
    public ItemMaster ComponentItemMaster { get; set; } = null!;

    public decimal Quantity { get; set; }
    public required string Uom { get; set; }
    public BomItemCategory ItemCategory { get; set; }
    public ProcurementType ProcurementType { get; set; }
    public decimal? ScrapPercent { get; set; }
    public string? ProcessRouteCode { get; set; }
    public string? PositionText { get; set; }
    public string? LineNotes { get; set; }
    public bool? IsPhantom { get; set; }
    public bool? IsBulk { get; set; }
    public bool? CutOnly { get; set; }
    public int SortOrder { get; set; }
}
