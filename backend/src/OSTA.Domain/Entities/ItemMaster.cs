namespace OSTA.Domain.Entities;

public class ItemMaster
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? MaterialCode { get; set; }
    public decimal? ThicknessMm { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? LengthMm { get; set; }
    public decimal? WidthMm { get; set; }
    public decimal? HeightMm { get; set; }
    public string? DrawingNumber { get; set; }
    public string? FinishCode { get; set; }
    public string? Specification { get; set; }
    public string? Notes { get; set; }
    public ItemType ItemType { get; set; }
    public ProcurementType ProcurementType { get; set; }
    public required string BaseUom { get; set; }
    public required string Revision { get; set; }
    public bool IsActive { get; set; }

    public ICollection<BomHeader> BomHeaders { get; set; } = new List<BomHeader>();
    public ICollection<BomItem> ComponentBomItems { get; set; } = new List<BomItem>();
    public ICollection<RoutingTemplate> RoutingTemplates { get; set; } = new List<RoutingTemplate>();
}
