using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OSTA.Infrastructure.Persistence;

namespace OSTA.Infrastructure.Migrations;

[DbContext(typeof(OstaDbContext))]
[Migration("20260330110000_AddConveyorLevelledTemplate")]
public class AddConveyorLevelledTemplate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            INSERT INTO bom_import_templates ("Id", "Code", "Name", "FormatType", "StructureType", "HeaderRowIndex", "DataStartRowIndex", "IsActive", "Notes")
            VALUES (
                '33333333-3333-3333-3333-333333333333',
                'CONVEYOR_BOM_LEVELLED_V1',
                'Conveyor BOM Levelled v1',
                'Csv',
                'Hierarchical',
                1,
                2,
                TRUE,
                'Level-aware conveyor BOM template. Level 0 defines the finished good; Level 1 rows become import lines.'
            )
            ON CONFLICT ("Code") DO UPDATE
            SET "Name" = EXCLUDED."Name",
                "FormatType" = EXCLUDED."FormatType",
                "StructureType" = EXCLUDED."StructureType",
                "HeaderRowIndex" = EXCLUDED."HeaderRowIndex",
                "DataStartRowIndex" = EXCLUDED."DataStartRowIndex",
                "IsActive" = EXCLUDED."IsActive",
                "Notes" = EXCLUDED."Notes";

            INSERT INTO bom_import_template_field_mappings ("Id", "BomImportTemplateId", "TargetField", "SourceColumnName", "DefaultValue", "IsRequired", "SortOrder")
            VALUES
                ('33333333-0000-0000-0000-000000000001', '33333333-3333-3333-3333-333333333333', 'ProjectCode', NULL, 'PRJ-CONVEYOR-LEVELLED-IMPORT', TRUE, 10),
                ('33333333-0000-0000-0000-000000000002', '33333333-3333-3333-3333-333333333333', 'ProjectName', NULL, 'Conveyor Levelled Import', TRUE, 20),
                ('33333333-0000-0000-0000-000000000003', '33333333-3333-3333-3333-333333333333', 'ParentItemCode', 'ParentCode', NULL, FALSE, 30),
                ('33333333-0000-0000-0000-000000000004', '33333333-3333-3333-3333-333333333333', 'ComponentItemCode', 'ComponentCode', NULL, TRUE, 40),
                ('33333333-0000-0000-0000-000000000005', '33333333-3333-3333-3333-333333333333', 'Description', 'Description', NULL, TRUE, 50),
                ('33333333-0000-0000-0000-000000000006', '33333333-3333-3333-3333-333333333333', 'Quantity', 'Qty', NULL, TRUE, 60),
                ('33333333-0000-0000-0000-000000000007', '33333333-3333-3333-3333-333333333333', 'BaseUom', 'UoM', NULL, FALSE, 70),
                ('33333333-0000-0000-0000-000000000008', '33333333-3333-3333-3333-333333333333', 'Revision', 'Revision', 'A', FALSE, 80),
                ('33333333-0000-0000-0000-000000000009', '33333333-3333-3333-3333-333333333333', 'MaterialCode', 'Material', NULL, FALSE, 90),
                ('33333333-0000-0000-0000-000000000010', '33333333-3333-3333-3333-333333333333', 'ThicknessMm', 'ThicknessMM', NULL, FALSE, 100),
                ('33333333-0000-0000-0000-000000000011', '33333333-3333-3333-3333-333333333333', 'WeightKg', 'WeightKG', NULL, FALSE, 110),
                ('33333333-0000-0000-0000-000000000012', '33333333-3333-3333-3333-333333333333', 'Notes', 'Notes', NULL, FALSE, 120)
            ON CONFLICT ("BomImportTemplateId", "TargetField") DO UPDATE
            SET "SourceColumnName" = EXCLUDED."SourceColumnName",
                "DefaultValue" = EXCLUDED."DefaultValue",
                "IsRequired" = EXCLUDED."IsRequired",
                "SortOrder" = EXCLUDED."SortOrder";
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM bom_import_template_field_mappings
            WHERE "BomImportTemplateId" = '33333333-3333-3333-3333-333333333333';

            DELETE FROM bom_import_templates
            WHERE "Id" = '33333333-3333-3333-3333-333333333333';
            """);
    }
}
