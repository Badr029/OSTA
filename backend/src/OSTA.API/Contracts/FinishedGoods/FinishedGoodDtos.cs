using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.FinishedGoods;

public sealed record FinishedGoodResponseDto(Guid Id, string Code, string Name, Guid ProjectId);

public sealed class CreateFinishedGoodRequestDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = default!;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = default!;
}
