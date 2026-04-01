namespace OSTA.Domain.Entities;

public class WorkOrderOperation
{
    public Guid Id { get; set; }

    public Guid WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;

    public Guid? RoutingOperationId { get; set; }
    public RoutingOperation? RoutingOperation { get; set; }

    public required string OperationNumber { get; set; }
    public required string OperationCode { get; set; }
    public required string OperationName { get; set; }

    public Guid WorkCenterId { get; set; }
    public WorkCenter WorkCenter { get; set; } = null!;

    public WorkOrderOperationStatus Status { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal CompletedQuantity { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int Sequence { get; set; }
    public bool IsQcGate { get; set; }
}
