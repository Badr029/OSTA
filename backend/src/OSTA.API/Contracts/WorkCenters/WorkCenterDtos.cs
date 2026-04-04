using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.WorkCenters;

public sealed record WorkCenterResponseDto(
    Guid Id,
    string Code,
    string Name,
    string Department,
    decimal HourlyRate,
    bool IsActive
);

public sealed record WorkCenterQueueItemDto(
    Guid WorkOrderId,
    string WorkOrderNumber,
    string WorkOrderStatus,
    string ProjectCode,
    string FinishedGoodCode,
    string AssemblyCode,
    Guid OperationId,
    string OperationNumber,
    string OperationCode,
    string OperationName,
    string OperationStatus,
    decimal PlannedQuantity,
    decimal CompletedQuantity,
    DateTime? ReleasedAtUtc,
    DateTime? StartedAtUtc,
    bool IsQcGate,
    int Sequence
);

public sealed class CreateWorkCenterRequestDto
{
    [Required]
    [StringLength(50)]
    public required string Code { get; set; }

    [Required]
    [StringLength(200)]
    public required string Name { get; set; }

    [Required]
    [StringLength(100)]
    public required string Department { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal HourlyRate { get; set; }

    public bool IsActive { get; set; }
}
