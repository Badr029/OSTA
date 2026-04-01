using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.BomImports;

public enum BomImportMode
{
    ExecutionOnly = 1,
    ExecutionAndProductDefinition = 2
}

public sealed class CreateBomImportBatchRequestDto
{
    [Required]
    [StringLength(260)]
    public required string SourceFileName { get; set; }

    [Required]
    [EnumDataType(typeof(BomImportMode))]
    public BomImportMode ImportMode { get; set; } = BomImportMode.ExecutionOnly;

    [Required]
    [MinLength(1)]
    public required List<CreateBomImportLineRequestDto> Lines { get; set; }
}

public sealed class CreateBomImportLineRequestDto
{
    [Range(1, int.MaxValue)]
    public int RowNumber { get; set; }

    [Required]
    [StringLength(50)]
    public required string ProjectCode { get; set; }

    [Required]
    [StringLength(200)]
    public required string ProjectName { get; set; }

    [Required]
    [StringLength(50)]
    public required string FinishedGoodCode { get; set; }

    [Required]
    [StringLength(200)]
    public required string FinishedGoodName { get; set; }

    [Required]
    [StringLength(50)]
    public required string AssemblyCode { get; set; }

    [Required]
    [StringLength(200)]
    public required string AssemblyName { get; set; }

    [Required]
    [StringLength(100)]
    public required string PartNumber { get; set; }

    [Required]
    [StringLength(30)]
    public required string Revision { get; set; }

    [Required]
    [StringLength(500)]
    public required string Description { get; set; }

    [Range(typeof(decimal), "0.0001", "999999999999")]
    public decimal Quantity { get; set; }

    [StringLength(100)]
    public string? MaterialCode { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? ThicknessMm { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? WeightKg { get; set; }

    [StringLength(100)]
    public string? DrawingNumber { get; set; }

    [StringLength(100)]
    public string? FinishCode { get; set; }

    [StringLength(500)]
    public string? Specification { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    [StringLength(100)]
    public string? ProcessRouteCode { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? ScrapPercent { get; set; }

    public bool? CutOnly { get; set; }
}

public sealed record BomImportBatchSummaryResponseDto(
    Guid Id,
    string SourceFileName,
    DateTimeOffset ImportedAtUtc,
    string Status,
    int TotalRows,
    int SuccessfulRows,
    int FailedRows
);

public sealed record BomImportLineResponseDto(
    Guid Id,
    int RowNumber,
    string ProjectCode,
    string ProjectName,
    string FinishedGoodCode,
    string FinishedGoodName,
    string AssemblyCode,
    string AssemblyName,
    string PartNumber,
    string Revision,
    string Description,
    decimal Quantity,
    string? MaterialCode,
    decimal? ThicknessMm,
    decimal? WeightKg,
    string? DrawingNumber,
    string? FinishCode,
    string? Specification,
    string? Notes,
    string? ProcessRouteCode,
    decimal? ScrapPercent,
    bool? CutOnly,
    string Status,
    string? ErrorMessage
);

public sealed record BomImportBatchResponseDto(
    Guid Id,
    string SourceFileName,
    DateTimeOffset ImportedAtUtc,
    string Status,
    int TotalRows,
    int SuccessfulRows,
    int FailedRows,
    IReadOnlyList<BomImportLineResponseDto> Lines
);

public sealed record BomImportLinePreviewResponseDto(
    int RowNumber,
    string ProjectCode,
    string ProjectName,
    string FinishedGoodCode,
    string FinishedGoodName,
    string AssemblyCode,
    string AssemblyName,
    string PartNumber,
    string Revision,
    string Description,
    decimal Quantity,
    string? MaterialCode,
    decimal? ThicknessMm,
    decimal? WeightKg,
    string? DrawingNumber,
    string? FinishCode,
    string? Specification,
    string? Notes,
    string? ProcessRouteCode,
    decimal? ScrapPercent,
    bool? CutOnly
);

public sealed record BomImportBatchPreviewResponseDto(
    string TemplateCode,
    string SourceFileName,
    string ImportMode,
    int TotalRows,
    IReadOnlyList<BomImportLinePreviewResponseDto> Lines
);
