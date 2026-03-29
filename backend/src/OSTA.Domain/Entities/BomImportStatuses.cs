namespace OSTA.Domain.Entities;

public enum BOMImportBatchStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}

public enum BOMImportLineStatus
{
    Pending = 1,
    Imported = 2,
    Failed = 3
}
