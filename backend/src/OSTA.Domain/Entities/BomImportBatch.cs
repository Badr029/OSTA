namespace OSTA.Domain.Entities;

public class BOMImportBatch
{
    public Guid Id { get; set; }
    public required string SourceFileName { get; set; }
    public DateTimeOffset ImportedAtUtc { get; set; }
    public BOMImportBatchStatus Status { get; set; }
    public int TotalRows { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }

    public ICollection<BOMImportLine> Lines { get; set; } = new List<BOMImportLine>();
}
