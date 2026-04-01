using System.ComponentModel.DataAnnotations;
using OSTA.Domain.Entities;

namespace OSTA.API.Contracts.RoutingTemplates;

public sealed record RoutingTemplateResponseDto(
    Guid Id,
    Guid ItemMasterId,
    string Code,
    string Name,
    string Revision,
    string Status,
    bool IsActive
);

public sealed class CreateRoutingTemplateRequestDto
{
    [Required]
    public Guid ItemMasterId { get; set; }

    [Required]
    [StringLength(100)]
    public required string Code { get; set; }

    [Required]
    [StringLength(200)]
    public required string Name { get; set; }

    [Required]
    [StringLength(30)]
    public required string Revision { get; set; }

    [Required]
    [EnumDataType(typeof(RoutingTemplateStatus))]
    public RoutingTemplateStatus Status { get; set; }

    public bool IsActive { get; set; }
}
