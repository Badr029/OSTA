namespace OSTA.Domain.Entities;

public class FinishedGood
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? SourceItemMasterId { get; set; }
    public ItemMaster? SourceItemMaster { get; set; }

    public Guid? SourceBomHeaderId { get; set; }
    public BomHeader? SourceBomHeader { get; set; }

    public ICollection<Assembly> Assemblies { get; set; } = new List<Assembly>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
