namespace OSTA.Domain.Entities;

public enum WorkOrderStatus
{
    Planned = 1,
    Released = 2,
    InProgress = 3,
    Completed = 4,
    Closed = 5,
    OnHold = 6
}

public enum WorkOrderOperationStatus
{
    Planned = 1,
    Ready = 2,
    InProgress = 3,
    Completed = 4,
    Blocked = 5,
    QcHold = 6
}
