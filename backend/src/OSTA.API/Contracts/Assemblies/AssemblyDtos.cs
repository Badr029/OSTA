using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.Assemblies;

public sealed record AssemblyResponseDto(
    Guid Id,
    string Code,
    string Name,
    Guid FinishedGoodId,
    Guid? SourceBomItemId,
    Guid? SourceComponentItemMasterId
);

public sealed record AssemblyMaterialReadinessRequirementResponseDto(
    Guid Id,
    string MaterialCode,
    decimal RequiredQuantity,
    string Uom,
    decimal? ThicknessMm,
    decimal? LengthMm,
    decimal? WidthMm,
    decimal? WeightKg,
    string? Notes
);

public sealed record AssemblyMaterialReadinessResponseDto(
    Guid AssemblyId,
    string AssemblyCode,
    Guid? SourceComponentItemMasterId,
    string? SourceComponentItemCode,
    bool IsMaterialReady,
    int MaterialRequirementCount,
    IReadOnlyList<string> MissingReasons,
    IReadOnlyList<AssemblyMaterialReadinessRequirementResponseDto> Requirements
);

public sealed class CreateAssemblyRequestDto
{
    [Required]
    [StringLength(50)]
    public required string Code { get; set; }

    [Required]
    [StringLength(200)]
    public required string Name { get; set; }
}
