namespace OSTA.Domain.Entities;

public class Assembly
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;

    public Guid FinishedGoodId { get; set; }
    public FinishedGood FinishedGood { get; set; } = default!;

    public ICollection<Part> Parts { get; set; } = new List<Part>();
}
