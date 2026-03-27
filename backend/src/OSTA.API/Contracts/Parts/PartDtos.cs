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
    public required string PartNumber { get; set; }

    [Required]
    [StringLength(30)]
    public required string Revision { get; set; }

    [Required]
    [StringLength(500)]
    public required string Description { get; set; }
}
