namespace OSTA.Domain.Entities;

public class ItemMaterialRequirement
{
    public Guid Id { get; set; }

    public Guid ItemMasterId { get; set; }
    public ItemMaster ItemMaster { get; set; } = null!;

    public required string MaterialCode { get; set; }
    public decimal RequiredQuantity { get; set; }
    public required string Uom { get; set; }
    public decimal? ThicknessMm { get; set; }
    public decimal? LengthMm { get; set; }
    public decimal? WidthMm { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Notes { get; set; }
}
