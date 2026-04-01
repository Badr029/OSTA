using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.WorkCenters;

public sealed record WorkCenterResponseDto(
    Guid Id,
    string Code,
    string Name,
    string Department,
    decimal HourlyRate,
    bool IsActive
);

public sealed class CreateWorkCenterRequestDto
{
    [Required]
    [StringLength(50)]
    public required string Code { get; set; }

    [Required]
    [StringLength(200)]
    public required string Name { get; set; }

    [Required]
    [StringLength(100)]
    public required string Department { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal HourlyRate { get; set; }

    public bool IsActive { get; set; }
}
