using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.BomImports;

public sealed class CreateBomImportBatchFromTemplateRequestDto
{
    [Required]
    [StringLength(100)]
    public required string TemplateCode { get; set; }

    [Required]
    [StringLength(260)]
    public required string SourceFileName { get; set; }

    [Required]
    [MinLength(1)]
    public required string CsvContent { get; set; }

    [Required]
    [EnumDataType(typeof(BomImportMode))]
    public BomImportMode ImportMode { get; set; } = BomImportMode.ExecutionOnly;

    public Dictionary<string, string>? DefaultValues { get; set; }
}
