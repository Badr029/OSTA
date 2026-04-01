using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.FinishedGoods;

public sealed record FinishedGoodResponseDto(
    Guid Id,
    string Code,
    string Name,
    Guid ProjectId,
    Guid? SourceItemMasterId,
    Guid? SourceBomHeaderId
);

public sealed class CreateFinishedGoodRequestDto
{
    [Required]
    [StringLength(50)]
    public required string Code { get; set; }

    [Required]
    [StringLength(200)]
    public required string Name { get; set; }
}
