namespace OSTA.Domain.Entities;

public class RoutingTemplate
{
    public Guid Id { get; set; }

    public Guid ItemMasterId { get; set; }
    public ItemMaster ItemMaster { get; set; } = null!;

    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Revision { get; set; }
    public RoutingTemplateStatus Status { get; set; }
    public bool IsActive { get; set; }

    public ICollection<RoutingOperation> Operations { get; set; } = new List<RoutingOperation>();
}
