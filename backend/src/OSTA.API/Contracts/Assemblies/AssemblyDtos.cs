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

public sealed class CreateAssemblyRequestDto
{
    [Required]
    [StringLength(50)]
    public required string Code { get; set; }

    [Required]
    [StringLength(200)]
    public required string Name { get; set; }
}
