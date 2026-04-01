namespace OSTA.Domain.Entities;

public class WorkCenter
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Department { get; set; }
    public decimal HourlyRate { get; set; }
    public bool IsActive { get; set; }

    public ICollection<RoutingOperation> RoutingOperations { get; set; } = new List<RoutingOperation>();
    public ICollection<WorkOrderOperation> WorkOrderOperations { get; set; } = new List<WorkOrderOperation>();
}
