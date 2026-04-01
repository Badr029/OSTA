namespace OSTA.API.Contracts.BomImportTemplates;

public sealed record BomImportTemplateSummaryResponseDto(
    Guid Id,
    string Code,
    string Name,
    string FormatType,
    string StructureType,
    int HeaderRowIndex,
    int DataStartRowIndex,
    bool IsActive
);

public sealed record BomImportTemplateFieldMappingResponseDto(
    Guid Id,
    string TargetField,
    string? SourceColumnName,
    string? DefaultValue,
    bool IsRequired,
    int SortOrder
);

public sealed record BomImportTemplateDetailResponseDto(
    Guid Id,
    string Code,
    string Name,
    string FormatType,
    string StructureType,
    int HeaderRowIndex,
    int DataStartRowIndex,
    bool IsActive,
    string? Notes,
    IReadOnlyList<BomImportTemplateFieldMappingResponseDto> FieldMappings
);
