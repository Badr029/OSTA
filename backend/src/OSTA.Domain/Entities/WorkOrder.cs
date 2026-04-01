namespace OSTA.Domain.Entities;

public class WorkOrder
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid FinishedGoodId { get; set; }
    public FinishedGood FinishedGood { get; set; } = null!;

    public Guid AssemblyId { get; set; }
    public Assembly Assembly { get; set; } = null!;

    public required string WorkOrderNumber { get; set; }
    public WorkOrderStatus Status { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal CompletedQuantity { get; set; }
    public DateTime? ReleasedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }

    public ICollection<WorkOrderOperation> Operations { get; set; } = new List<WorkOrderOperation>();
}
