namespace OSTA.Domain.Entities;

public class Part
{
    public Guid Id { get; set; }
    public required string PartNumber { get; set; }
    public required string Revision { get; set; }
    public required string Description { get; set; }

    public Guid AssemblyId { get; set; }
    public Assembly Assembly { get; set; } = null!;
}
