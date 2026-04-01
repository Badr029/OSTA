
namespace OSTA.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }

    public ICollection<FinishedGood> FinishedGoods { get; set; } = new List<FinishedGood>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}
