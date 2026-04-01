namespace OSTA.Domain.Entities;

public class Assembly
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }

    public Guid FinishedGoodId { get; set; }
    public FinishedGood FinishedGood { get; set; } = null!;

    public Guid? SourceBomItemId { get; set; }
    public BomItem? SourceBomItem { get; set; }

    public Guid? SourceComponentItemMasterId { get; set; }
    public ItemMaster? SourceComponentItemMaster { get; set; }

    public ICollection<Part> Parts { get; set; } = new List<Part>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
