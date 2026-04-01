namespace OSTA.Domain.Entities;

public class RoutingOperation
{
    public Guid Id { get; set; }

    public Guid RoutingTemplateId { get; set; }
    public RoutingTemplate RoutingTemplate { get; set; } = null!;

    public required string OperationNumber { get; set; }
    public required string OperationCode { get; set; }
    public required string OperationName { get; set; }

    public Guid WorkCenterId { get; set; }
    public WorkCenter WorkCenter { get; set; } = null!;

    public decimal SetupTimeMinutes { get; set; }
    public decimal RunTimeMinutes { get; set; }
    public int Sequence { get; set; }
    public bool IsQcGate { get; set; }

    public ICollection<WorkOrderOperation> WorkOrderOperations { get; set; } = new List<WorkOrderOperation>();
}
