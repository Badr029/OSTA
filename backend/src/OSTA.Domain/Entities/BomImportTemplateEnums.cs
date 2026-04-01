namespace OSTA.Domain.Entities;

public enum BomImportTemplateFormatType
{
    Csv = 1,
    Excel = 2
}

public enum BomImportTemplateStructureType
{
    Flat = 1,
    Hierarchical = 2
}

public enum BomImportTemplateTargetField
{
    ProjectCode = 1,
    ProjectName = 2,
    FinishedGoodCode = 3,
    FinishedGoodName = 4,
    AssemblyCode = 5,
    AssemblyName = 6,
    PartNumber = 7,
    Revision = 8,
    Description = 9,
    Quantity = 10,
    BaseUom = 11,
    MaterialCode = 12,
    ThicknessMm = 13,
    ProcessRoute = 14,
    CutOnly = 15,
    WeightKg = 16,
    Notes = 17,
    ParentItemCode = 18,
    ParentItemName = 19,
    ComponentItemCode = 20,
    ComponentItemName = 21,
    DrawingNumber = 22,
    FinishCode = 23,
    Specification = 24,
    ProcessRouteCode = 25,
    ScrapPercent = 26
}
