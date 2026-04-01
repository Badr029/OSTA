using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OSTA.Infrastructure.Persistence;

namespace OSTA.Infrastructure.Migrations;

[DbContext(typeof(OstaDbContext))]
[Migration("20260330094500_AdjustConveyorTemplateMappings")]
public class AdjustConveyorTemplateMappings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE bom_import_templates
            SET "FormatType" = 'Csv'
            WHERE "Id" = '22222222-2222-2222-2222-222222222222';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'FinishedGoodCode', "SourceColumnName" = 'ParentItemCode', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 30
            WHERE "Id" = '22222222-0000-0000-0000-000000000003';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'FinishedGoodName', "SourceColumnName" = 'ParentItemName', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 40
            WHERE "Id" = '22222222-0000-0000-0000-000000000004';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'AssemblyCode', "SourceColumnName" = 'ComponentItemCode', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 50
            WHERE "Id" = '22222222-0000-0000-0000-000000000005';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'AssemblyName', "SourceColumnName" = 'ComponentItemName', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 60
            WHERE "Id" = '22222222-0000-0000-0000-000000000006';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'PartNumber', "SourceColumnName" = 'ComponentItemCode', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 70
            WHERE "Id" = '22222222-0000-0000-0000-000000000007';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'Revision', "SourceColumnName" = NULL, "DefaultValue" = 'A', "IsRequired" = TRUE, "SortOrder" = 80
            WHERE "Id" = '22222222-0000-0000-0000-000000000008';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'Description', "SourceColumnName" = 'ComponentItemName', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 90
            WHERE "Id" = '22222222-0000-0000-0000-000000000009';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'Quantity', "SourceColumnName" = 'Quantity', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 100
            WHERE "Id" = '22222222-0000-0000-0000-000000000010';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE bom_import_templates
            SET "FormatType" = 'Excel'
            WHERE "Id" = '22222222-2222-2222-2222-222222222222';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'ParentItemCode', "SourceColumnName" = 'ParentItemCode', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 30
            WHERE "Id" = '22222222-0000-0000-0000-000000000003';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'ParentItemName', "SourceColumnName" = 'ParentItemName', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 40
            WHERE "Id" = '22222222-0000-0000-0000-000000000004';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'ComponentItemCode', "SourceColumnName" = 'ComponentItemCode', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 50
            WHERE "Id" = '22222222-0000-0000-0000-000000000005';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'ComponentItemName', "SourceColumnName" = 'ComponentItemName', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 60
            WHERE "Id" = '22222222-0000-0000-0000-000000000006';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'PartNumber', "SourceColumnName" = 'PartNumber', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 70
            WHERE "Id" = '22222222-0000-0000-0000-000000000007';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'Revision', "SourceColumnName" = NULL, "DefaultValue" = 'A', "IsRequired" = TRUE, "SortOrder" = 80
            WHERE "Id" = '22222222-0000-0000-0000-000000000008';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'Description', "SourceColumnName" = 'Description', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 90
            WHERE "Id" = '22222222-0000-0000-0000-000000000009';

            UPDATE bom_import_template_field_mappings
            SET "TargetField" = 'Quantity', "SourceColumnName" = 'Quantity', "DefaultValue" = NULL, "IsRequired" = TRUE, "SortOrder" = 100
            WHERE "Id" = '22222222-0000-0000-0000-000000000010';
            """);
    }
}
