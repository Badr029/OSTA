using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.Parts;

public sealed record PartResponseDto(
    Guid Id,
    string PartNumber,
    string Revision,
    string Description,
    Guid AssemblyId
);

public sealed class CreatePartRequestDto
{
    [Required]
    [StringLength(100)]
    public string PartNumber { get; set; } = default!;

    [Required]
    [StringLength(30)]
    public string Revision { get; set; } = default!;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = default!;
}
