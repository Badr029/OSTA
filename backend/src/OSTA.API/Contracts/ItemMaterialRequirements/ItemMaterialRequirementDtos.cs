using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.ItemMaterialRequirements;

public sealed record ItemMaterialRequirementResponseDto(
    Guid Id,
    Guid ItemMasterId,
    string MaterialCode,
    decimal RequiredQuantity,
    string Uom,
    decimal? ThicknessMm,
    decimal? LengthMm,
    decimal? WidthMm,
    decimal? WeightKg,
    string? Notes
);

public sealed class CreateItemMaterialRequirementRequestDto
{
    [Required]
    [StringLength(100)]
    public required string MaterialCode { get; set; }

    [Range(typeof(decimal), "0.0001", "999999999999")]
    public decimal RequiredQuantity { get; set; }

    [Required]
    [StringLength(20)]
    public required string Uom { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? ThicknessMm { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? LengthMm { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? WidthMm { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? WeightKg { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}

public sealed class UpdateItemMaterialRequirementRequestDto
{
    [Required]
    [StringLength(100)]
    public required string MaterialCode { get; set; }

    [Range(typeof(decimal), "0.0001", "999999999999")]
    public decimal RequiredQuantity { get; set; }

    [Required]
    [StringLength(20)]
    public required string Uom { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? ThicknessMm { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? LengthMm { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? WidthMm { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? WeightKg { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}
