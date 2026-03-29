namespace OSTA.Domain.Entities;

public class BOMImportLine
{
    public Guid Id { get; set; }

    public Guid BOMImportBatchId { get; set; }
    public BOMImportBatch BOMImportBatch { get; set; } = null!;

    public int RowNumber { get; set; }
    public required string ProjectCode { get; set; }
    public required string ProjectName { get; set; }
    public required string FinishedGoodCode { get; set; }
    public required string FinishedGoodName { get; set; }
    public required string AssemblyCode { get; set; }
    public required string AssemblyName { get; set; }
    public required string PartNumber { get; set; }
    public required string Revision { get; set; }
    public required string Description { get; set; }
    public decimal Quantity { get; set; }
    public BOMImportLineStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
}
