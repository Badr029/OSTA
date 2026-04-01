namespace OSTA.API.Contracts.WorkOrderOperations;

public sealed record WorkOrderOperationResponseDto(
    Guid Id,
    Guid WorkOrderId,
    Guid? RoutingOperationId,
    string OperationNumber,
    string OperationCode,
    string OperationName,
    Guid WorkCenterId,
    string WorkCenterCode,
    string Status,
    decimal PlannedQuantity,
    decimal CompletedQuantity,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    int Sequence,
    bool IsQcGate
);
