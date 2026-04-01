using System.ComponentModel.DataAnnotations;
using OSTA.Domain.Entities;

namespace OSTA.API.Contracts.BomItems;

public sealed record BomItemResponseDto(
    Guid Id,
    Guid BomHeaderId,
    string ItemNumber,
    Guid ComponentItemMasterId,
    string ComponentItemCode,
    string ComponentItemName,
    decimal Quantity,
    string Uom,
    string ItemCategory,
    string ProcurementType,
    decimal? ScrapPercent,
    string? ProcessRouteCode,
    string? PositionText,
    string? LineNotes,
    bool? IsPhantom,
    bool? IsBulk,
    bool? CutOnly,
    int SortOrder
);

public sealed class CreateBomItemRequestDto
{
    [Required]
    [StringLength(20)]
    public required string ItemNumber { get; set; }

    [Required]
    public Guid ComponentItemMasterId { get; set; }

    [Range(typeof(decimal), "0.0001", "999999999999")]
    public decimal Quantity { get; set; }

    [Required]
    [StringLength(20)]
    public required string Uom { get; set; }

    [Required]
    [EnumDataType(typeof(BomItemCategory))]
    public BomItemCategory ItemCategory { get; set; }

    [Required]
    [EnumDataType(typeof(ProcurementType))]
    public ProcurementType ProcurementType { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? ScrapPercent { get; set; }

    [StringLength(100)]
    public string? ProcessRouteCode { get; set; }

    [StringLength(200)]
    public string? PositionText { get; set; }

    [StringLength(2000)]
    public string? LineNotes { get; set; }

    public bool? IsPhantom { get; set; }

    public bool? IsBulk { get; set; }

    public bool? CutOnly { get; set; }

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }
}
