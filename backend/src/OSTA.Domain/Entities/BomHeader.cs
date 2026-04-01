namespace OSTA.Domain.Entities;

public class BomHeader
{
    public Guid Id { get; set; }

    public Guid ParentItemMasterId { get; set; }
    public ItemMaster ParentItemMaster { get; set; } = null!;

    public required string Revision { get; set; }
    public decimal BaseQuantity { get; set; }
    public BomUsage Usage { get; set; }
    public BomHeaderStatus Status { get; set; }
    public required string PlantCode { get; set; }

    public ICollection<BomItem> Items { get; set; } = new List<BomItem>();
}
