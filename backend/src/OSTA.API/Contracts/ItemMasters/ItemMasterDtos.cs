using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.ItemMasters;

public sealed record ItemMasterResponseDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    string? MaterialCode,
    decimal? ThicknessMm,
    decimal? WeightKg,
    decimal? LengthMm,
    decimal? WidthMm,
    decimal? HeightMm,
    string? DrawingNumber,
    string? FinishCode,
    string? Specification,
    string? Notes,
    string ItemType,
    string ProcurementType,
    string BaseUom,
    string Revision,
    bool IsActive
);

public sealed class CreateItemMasterRequestDto
{
    [Required]
    [StringLength(100)]
    public required string Code { get; set; }

    [Required]
    [StringLength(200)]
    public required string Name { get; set; }

    [Required]
    [StringLength(500)]
    public required string Description { get; set; }

    [StringLength(100)]
    public string? MaterialCode { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? ThicknessMm { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? WeightKg { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? LengthMm { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? WidthMm { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? HeightMm { get; set; }

    [StringLength(100)]
    public string? DrawingNumber { get; set; }

    [StringLength(100)]
    public string? FinishCode { get; set; }

    [StringLength(500)]
    public string? Specification { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    [Required]
    [EnumDataType(typeof(OSTA.Domain.Entities.ItemType))]
    public OSTA.Domain.Entities.ItemType ItemType { get; set; }

    [Required]
    [EnumDataType(typeof(OSTA.Domain.Entities.ProcurementType))]
    public OSTA.Domain.Entities.ProcurementType ProcurementType { get; set; }

    [Required]
    [StringLength(20)]
    public required string BaseUom { get; set; }

    [Required]
    [StringLength(30)]
    public required string Revision { get; set; }

    public bool IsActive { get; set; }
}
