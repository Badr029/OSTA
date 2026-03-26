namespace OSTA.Domain.Entities;

public class FinishedGood
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = default!;

    public ICollection<Assembly> Assemblies { get; set; } = new List<Assembly>();
}
