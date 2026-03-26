
namespace OSTA.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;

    public ICollection<FinishedGood> FinishedGoods { get; set; } = new List<FinishedGood>();
}
