using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.WorkOrders;

public sealed record WorkOrderResponseDto(
    Guid Id,
    string WorkOrderNumber,
    Guid ProjectId,
    string ProjectCode,
    Guid FinishedGoodId,
    string FinishedGoodCode,
    Guid AssemblyId,
    string AssemblyCode,
    string Status,
    decimal PlannedQuantity,
    decimal CompletedQuantity,
    DateTime? ReleasedAtUtc,
    DateTime? ClosedAtUtc,
    int OperationCount
);

public sealed class GenerateWorkOrderRequestDto
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public Guid FinishedGoodId { get; set; }

    [Required]
    public Guid AssemblyId { get; set; }

    [Range(typeof(decimal), "0.0001", "999999999999")]
    public decimal? PlannedQuantity { get; set; }
}
