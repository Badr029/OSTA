using System.ComponentModel.DataAnnotations;
using OSTA.Domain.Entities;

namespace OSTA.API.Contracts.BomHeaders;

public sealed record BomHeaderResponseDto(
    Guid Id,
    Guid ParentItemMasterId,
    string Revision,
    decimal BaseQuantity,
    string Usage,
    string Status,
    string PlantCode
);

public sealed record BomHeaderStructureResponseDto(
    Guid Id,
    Guid ParentItemMasterId,
    string ParentItemCode,
    string ParentItemName,
    string ParentItemType,
    string Revision,
    decimal BaseQuantity,
    string Usage,
    string Status,
    string PlantCode,
    IReadOnlyList<BomHeaderStructureItemResponseDto> Items
);

public sealed record BomHeaderStructureItemResponseDto(
    Guid Id,
    string ItemNumber,
    Guid ComponentItemMasterId,
    string ComponentItemCode,
    string ComponentItemName,
    string ComponentItemType,
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

public sealed class CreateBomHeaderRequestDto
{
    [Required]
    public Guid ParentItemMasterId { get; set; }

    [Required]
    [StringLength(30)]
    public required string Revision { get; set; }

    [Range(typeof(decimal), "0.0001", "999999999999")]
    public decimal BaseQuantity { get; set; }

    [Required]
    [EnumDataType(typeof(BomUsage))]
    public BomUsage Usage { get; set; }

    [Required]
    [EnumDataType(typeof(BomHeaderStatus))]
    public BomHeaderStatus Status { get; set; }

    [Required]
    [StringLength(50)]
    public required string PlantCode { get; set; }
}
