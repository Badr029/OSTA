namespace OSTA.Domain.Entities;

public class Part
{
    public Guid Id { get; set; }
    public string PartNumber { get; set; } = default!;
    public string Revision { get; set; } = default!;
    public string Description { get; set; } = default!;

    public Guid AssemblyId { get; set; }
    public Assembly Assembly { get; set; } = default!;
}
