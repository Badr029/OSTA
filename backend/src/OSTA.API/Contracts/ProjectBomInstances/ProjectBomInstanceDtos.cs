using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.ProjectBomInstances;

public sealed class GenerateProjectStructureFromBomRequestDto
{
    [Required]
    public Guid BomHeaderId { get; set; }
}

public sealed record ProjectBomInstanceResponseDto(
    Guid ProjectId,
    Guid BomHeaderId,
    Guid SourceParentItemMasterId,
    Guid FinishedGoodId,
    string FinishedGoodCode,
    string FinishedGoodName,
    string FinishedGoodAction,
    IReadOnlyList<ProjectBomInstanceAssemblyResponseDto> Assemblies
);

public sealed record ProjectBomInstanceAssemblyResponseDto(
    Guid AssemblyId,
    string Code,
    string Name,
    string Action,
    Guid SourceBomItemId,
    Guid SourceComponentItemMasterId
);
