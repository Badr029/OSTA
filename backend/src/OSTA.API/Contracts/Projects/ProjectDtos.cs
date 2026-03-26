using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.Projects;

public sealed record ProjectResponseDto(Guid Id, string Code, string Name);

public sealed class CreateProjectRequestDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = default!;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = default!;
}
