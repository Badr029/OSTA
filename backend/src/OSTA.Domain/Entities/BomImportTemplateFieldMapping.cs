namespace OSTA.Domain.Entities;

public class BomImportTemplateFieldMapping
{
    public Guid Id { get; set; }

    public Guid BomImportTemplateId { get; set; }
    public BomImportTemplate BomImportTemplate { get; set; } = null!;

    public BomImportTemplateTargetField TargetField { get; set; }
    public string? SourceColumnName { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsRequired { get; set; }
    public int SortOrder { get; set; }
}
