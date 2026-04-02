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

public sealed record WorkOrderReleaseReadinessResponseDto(
    Guid WorkOrderId,
    string WorkOrderNumber,
    string WorkOrderStatus,
    Guid AssemblyId,
    string AssemblyCode,
    bool HasOperations,
    int OperationCount,
    bool IsMaterialReady,
    bool IsReleaseReady,
    IReadOnlyList<string> BlockingReasons
);

public sealed record WorkOrderOperationSummaryDto(
    Guid Id,
    string OperationNumber,
    string OperationCode,
    string OperationName,
    string WorkCenterCode,
    string Status,
    int Sequence,
    bool IsQcGate
);

public sealed record WorkOrderSummaryResponseDto(
    Guid WorkOrderId,
    string WorkOrderNumber,
    string Status,
    string ProjectCode,
    string FinishedGoodCode,
    string AssemblyCode,
    decimal PlannedQuantity,
    decimal CompletedQuantity,
    bool IsReleaseReady,
    bool IsMaterialReady,
    int TotalOperations,
    int CompletedOperationsCount,
    int BlockedOperationsCount,
    int InProgressOperationsCount,
    WorkOrderOperationSummaryDto? CurrentOperation,
    WorkOrderOperationSummaryDto? NextOperation,
    DateTime? ReleasedAtUtc,
    DateTime? ClosedAtUtc
);

public sealed record WorkOrderSummaryListItemDto(
    Guid WorkOrderId,
    string WorkOrderNumber,
    string Status,
    string ProjectCode,
    string FinishedGoodCode,
    string AssemblyCode,
    decimal PlannedQuantity,
    decimal CompletedQuantity,
    bool IsReleaseReady,
    bool IsMaterialReady,
    string? CurrentOperationCode,
    string? CurrentOperationStatus,
    string? NextOperationCode,
    DateTime? ReleasedAtUtc
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
