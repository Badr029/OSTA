using System.ComponentModel.DataAnnotations;

namespace OSTA.API.Contracts.RoutingOperations;

public sealed record RoutingOperationResponseDto(
    Guid Id,
    Guid RoutingTemplateId,
    string OperationNumber,
    string OperationCode,
    string OperationName,
    Guid WorkCenterId,
    string WorkCenterCode,
    string WorkCenterName,
    decimal SetupTimeMinutes,
    decimal RunTimeMinutes,
    int Sequence,
    bool IsQcGate
);

public sealed class CreateRoutingOperationRequestDto
{
    [Required]
    [StringLength(20)]
    public required string OperationNumber { get; set; }

    [Required]
    [StringLength(50)]
    public required string OperationCode { get; set; }

    [Required]
    [StringLength(200)]
    public required string OperationName { get; set; }

    [Required]
    public Guid WorkCenterId { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal SetupTimeMinutes { get; set; }

    [Range(typeof(decimal), "0", "999999999999")]
    public decimal RunTimeMinutes { get; set; }

    [Range(1, int.MaxValue)]
    public int Sequence { get; set; }

    public bool IsQcGate { get; set; }
}
