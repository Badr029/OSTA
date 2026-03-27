namespace OSTA.Domain.Entities;

public class FinishedGood
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ICollection<Assembly> Assemblies { get; set; } = new List<Assembly>();
}
