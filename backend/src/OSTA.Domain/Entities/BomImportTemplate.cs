namespace OSTA.Domain.Entities;

public class BomImportTemplate
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public BomImportTemplateFormatType FormatType { get; set; }
    public BomImportTemplateStructureType StructureType { get; set; }
    public int HeaderRowIndex { get; set; }
    public int DataStartRowIndex { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    public ICollection<BomImportTemplateFieldMapping> FieldMappings { get; set; } = new List<BomImportTemplateFieldMapping>();
}
