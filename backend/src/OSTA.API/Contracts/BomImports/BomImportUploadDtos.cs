using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.BomImports;

public sealed class PreviewBomImportUploadRequestDto
{
    [Required]
    [StringLength(100)]
    public required string TemplateCode { get; set; }

    public string? DefaultValuesJson { get; set; }
}

public sealed class CreateBomImportUploadRequestDto
{
    [Required]
    [StringLength(100)]
    public required string TemplateCode { get; set; }

    [Required]
    [EnumDataType(typeof(BomImportMode))]
    public BomImportMode ImportMode { get; set; } = BomImportMode.ExecutionOnly;

    public string? DefaultValuesJson { get; set; }
}
