using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.Assemblies;

public sealed record AssemblyResponseDto(Guid Id, string Code, string Name, Guid FinishedGoodId);

public sealed class CreateAssemblyRequestDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = default!;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = default!;
}
