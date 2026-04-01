using OSTA.Domain.Entities;

namespace OSTA.API.Imports;

internal static class BomImportTemplateCatalog
{
    private static readonly BomImportTemplateDefinition QccMtoCsvV1 = new(
        "QCC_MTO_CSV_V1",
        BomImportTemplateFormatType.Csv,
        BomImportTemplateStructureType.Flat,
        1,
        2,
        ',',
        [
            new(BomImportTemplateTargetField.ProjectCode, ["ProjectCode", "PROJECT_CODE", "Project Code"], true),
            new(BomImportTemplateTargetField.ProjectName, ["ProjectName", "PROJECT_NAME", "Project Name"], true),
            new(BomImportTemplateTargetField.FinishedGoodCode, ["FinishedGoodCode", "FGCode", "FG_CODE", "FG Code", "Finished Good Code"], true),
            new(BomImportTemplateTargetField.FinishedGoodName, ["FinishedGoodName", "FGName", "FG_NAME", "FG Name", "Finished Good Name"], true),
            new(BomImportTemplateTargetField.AssemblyCode, ["AssemblyCode", "ASSY_CODE", "Assembly Code"], true),
            new(BomImportTemplateTargetField.AssemblyName, ["AssemblyName", "ASSY_NAME", "Assembly Name"], true),
            new(BomImportTemplateTargetField.PartNumber, ["PartNumber", "PART_NUMBER", "Part Number", "PartNo", "Part No"], true),
            new(BomImportTemplateTargetField.Revision, ["Revision", "REVISION", "Rev", "REV"], true),
            new(BomImportTemplateTargetField.Description, ["Description", "DESCRIPTION", "Part Description", "Desc"], true),
            new(BomImportTemplateTargetField.Quantity, ["Quantity", "QTY", "Qty", "QUANTITY"], true),
            new(BomImportTemplateTargetField.BaseUom, ["BaseUom", "UOM", "Uom"], false),
            new(BomImportTemplateTargetField.MaterialCode, ["MaterialCode", "MATERIAL_CODE", "Material Code"], false),
            new(BomImportTemplateTargetField.ThicknessMm, ["ThicknessMm", "THICKNESS_MM", "Thickness"], false),
            new(BomImportTemplateTargetField.ProcessRouteCode, ["ProcessRouteCode", "PROCESS_ROUTE_CODE", "Process Route"], false),
            new(BomImportTemplateTargetField.CutOnly, ["CutOnly", "CUT_ONLY", "Cut Only"], false),
            new(BomImportTemplateTargetField.WeightKg, ["WeightKg", "WEIGHT_KG", "Weight"], false),
            new(BomImportTemplateTargetField.DrawingNumber, ["DrawingNumber", "DRAWING_NUMBER", "Drawing No"], false),
            new(BomImportTemplateTargetField.FinishCode, ["FinishCode", "FINISH_CODE", "Finish Code"], false),
            new(BomImportTemplateTargetField.Specification, ["Specification", "SPECIFICATION", "Spec"], false),
            new(BomImportTemplateTargetField.ScrapPercent, ["ScrapPercent", "SCRAP_PERCENT", "Scrap %"], false),
            new(BomImportTemplateTargetField.Notes, ["Notes", "NOTES", "Line Notes"], false)
        ],
        new Dictionary<BomImportTemplateTargetField, string>()
    );

    private static readonly BomImportTemplateDefinition ConveyorBomV1 = new(
        "CONVEYOR_BOM_V1",
        BomImportTemplateFormatType.Csv,
        BomImportTemplateStructureType.Hierarchical,
        1,
        2,
        ',',
        [
            new(BomImportTemplateTargetField.ProjectCode, ["ProjectCode", "PROJECT_CODE", "Project Code"], true),
            new(BomImportTemplateTargetField.ProjectName, ["ProjectName", "PROJECT_NAME", "Project Name"], true),
            new(BomImportTemplateTargetField.FinishedGoodCode, ["ParentItemCode", "Parent Item Code", "PARENT_ITEM_CODE"], true),
            new(BomImportTemplateTargetField.FinishedGoodName, ["ParentItemName", "Parent Item Name", "PARENT_ITEM_NAME"], true),
            new(BomImportTemplateTargetField.AssemblyCode, ["ComponentItemCode", "Component Item Code", "COMPONENT_ITEM_CODE"], true),
            new(BomImportTemplateTargetField.AssemblyName, ["ComponentItemName", "Component Item Name", "COMPONENT_ITEM_NAME"], true),
            new(BomImportTemplateTargetField.PartNumber, ["ComponentItemCode", "Component Item Code", "COMPONENT_ITEM_CODE", "PartNumber", "PART_NUMBER"], true),
            new(BomImportTemplateTargetField.Revision, ["Revision", "REVISION", "Rev", "REV"], true),
            new(BomImportTemplateTargetField.Description, ["ComponentItemName", "Component Item Name", "COMPONENT_ITEM_NAME", "Description", "DESCRIPTION"], true),
            new(BomImportTemplateTargetField.Quantity, ["Quantity", "Qty", "QTY"], true)
        ],
        new Dictionary<BomImportTemplateTargetField, string>
        {
            [BomImportTemplateTargetField.ProjectCode] = "PRJ-CONVEYOR-IMPORT",
            [BomImportTemplateTargetField.ProjectName] = "Conveyor Template Import",
            [BomImportTemplateTargetField.Revision] = "A"
        }
    );

    private static readonly BomImportTemplateDefinition ConveyorBomLevelledV1 = new(
        "CONVEYOR_BOM_LEVELLED_V1",
        BomImportTemplateFormatType.Csv,
        BomImportTemplateStructureType.Hierarchical,
        1,
        2,
        ',',
        [
            new(BomImportTemplateTargetField.ProjectCode, ["ProjectCode", "PROJECT_CODE", "Project Code"], true),
            new(BomImportTemplateTargetField.ProjectName, ["ProjectName", "PROJECT_NAME", "Project Name"], true),
            new(BomImportTemplateTargetField.ParentItemCode, ["ParentCode", "Parent Code"], false),
            new(BomImportTemplateTargetField.ComponentItemCode, ["ComponentCode", "Component Code"], true),
            new(BomImportTemplateTargetField.Description, ["Description", "DESCRIPTION"], true),
            new(BomImportTemplateTargetField.Quantity, ["Qty", "QTY", "Quantity"], true),
            new(BomImportTemplateTargetField.BaseUom, ["UoM", "UOM"], false),
            new(BomImportTemplateTargetField.Revision, ["Revision", "REVISION"], false),
            new(BomImportTemplateTargetField.MaterialCode, ["Material", "MATERIAL"], false),
            new(BomImportTemplateTargetField.ThicknessMm, ["ThicknessMM", "ThicknessMm", "THICKNESS_MM"], false),
            new(BomImportTemplateTargetField.WeightKg, ["WeightKG", "WeightKg", "WEIGHT_KG"], false),
            new(BomImportTemplateTargetField.Notes, ["Notes", "NOTES"], false)
        ],
        new Dictionary<BomImportTemplateTargetField, string>
        {
            [BomImportTemplateTargetField.ProjectCode] = "PRJ-CONVEYOR-LEVELLED-IMPORT",
            [BomImportTemplateTargetField.ProjectName] = "Conveyor Levelled Import",
            [BomImportTemplateTargetField.Revision] = "A"
        }
    );

    private static readonly IReadOnlyDictionary<string, BomImportTemplateDefinition> Templates =
        new Dictionary<string, BomImportTemplateDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["QCC_MTO_CSV_V1"] = QccMtoCsvV1,
            ["CONVEYOR_BOM_V1"] = ConveyorBomV1,
            ["CONVEYOR_BOM_LEVELLED_V1"] = ConveyorBomLevelledV1
        };

    public static BomImportTemplateDefinition GetRequired(string templateCode)
    {
        if (!Templates.TryGetValue(templateCode, out var template))
        {
            throw new BomImportTemplateMappingException(
                $"Template '{templateCode}' was not found. Supported templates: {string.Join(", ", Templates.Keys.OrderBy(x => x))}.");
        }

        return template;
    }
}
