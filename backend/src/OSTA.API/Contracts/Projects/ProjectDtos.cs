using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.Projects;

public sealed record ProjectResponseDto(Guid Id, string Code, string Name);

public sealed class CreateProjectRequestDto
{
    [Required]
    [StringLength(50)]
    public required string Code { get; set; }

    [Required]
    [StringLength(200)]
    public required string Name { get; set; }
}
