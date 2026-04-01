using OSTA.Domain.Entities;

namespace OSTA.API.Imports;

internal sealed record BomImportTemplateDefinition(
    string TemplateCode,
    BomImportTemplateFormatType FormatType,
    BomImportTemplateStructureType StructureType,
    int HeaderRowIndex,
    int DataStartRowIndex,
    char Delimiter,
    IReadOnlyList<BomImportTemplateColumnMapping> ColumnMappings,
    IReadOnlyDictionary<BomImportTemplateTargetField, string> DefaultValues
);

internal sealed record BomImportTemplateColumnMapping(
    BomImportTemplateTargetField TargetField,
    IReadOnlyList<string> SourceColumns,
    bool IsRequired
);

internal sealed class BomImportTemplateMappingException : Exception
{
    public BomImportTemplateMappingException(string message)
        : base(message)
    {
    }
}
