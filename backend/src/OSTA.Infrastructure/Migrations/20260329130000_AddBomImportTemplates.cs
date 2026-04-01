using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSTA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBomImportTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bom_import_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FormatType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StructureType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HeaderRowIndex = table.Column<int>(type: "integer", nullable: false),
                    DataStartRowIndex = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bom_import_templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bom_import_template_field_mappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BomImportTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetField = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SourceColumnName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DefaultValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bom_import_template_field_mappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bom_import_template_field_mappings_bom_import_templates_BomImportTemplateId",
                        column: x => x.BomImportTemplateId,
                        principalTable: "bom_import_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bom_import_template_field_mappings_BomImportTemplateId_TargetField",
                table: "bom_import_template_field_mappings",
                columns: new[] { "BomImportTemplateId", "TargetField" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bom_import_templates_Code",
                table: "bom_import_templates",
                column: "Code",
                unique: true);

            migrationBuilder.InsertData(
                table: "bom_import_templates",
                columns: new[] { "Id", "Code", "Name", "FormatType", "StructureType", "HeaderRowIndex", "DataStartRowIndex", "IsActive", "Notes" },
                values: new object[,]
                {
                    {
                        new Guid("11111111-1111-1111-1111-111111111111"),
                        "QCC_MTO_CSV_V1",
                        "QCC MTO CSV v1",
                        "Csv",
                        "Flat",
                        1,
                        2,
                        true,
                        "Flat CSV template for QCC MTO imports."
                    },
                    {
                        new Guid("22222222-2222-2222-2222-222222222222"),
                        "CONVEYOR_BOM_V1",
                        "Conveyor BOM v1",
                        "Excel",
                        "Hierarchical",
                        1,
                        2,
                        true,
                        "Hierarchical conveyor BOM template mapped from parent/component fields."
                    }
                });

            migrationBuilder.InsertData(
                table: "bom_import_template_field_mappings",
                columns: new[] { "Id", "BomImportTemplateId", "TargetField", "SourceColumnName", "DefaultValue", "IsRequired", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), "ProjectCode", "project_code", null, true, 10 },
                    { new Guid("11111111-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), "ProjectName", "project_code", null, true, 20 },
                    { new Guid("11111111-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), "FinishedGoodCode", "project_code", null, true, 30 },
                    { new Guid("11111111-0000-0000-0000-000000000004"), new Guid("11111111-1111-1111-1111-111111111111"), "FinishedGoodName", "project_code", null, true, 40 },
                    { new Guid("11111111-0000-0000-0000-000000000005"), new Guid("11111111-1111-1111-1111-111111111111"), "AssemblyCode", "assembly_code", null, true, 50 },
                    { new Guid("11111111-0000-0000-0000-000000000006"), new Guid("11111111-1111-1111-1111-111111111111"), "AssemblyName", "assembly_code", null, true, 60 },
                    { new Guid("11111111-0000-0000-0000-000000000007"), new Guid("11111111-1111-1111-1111-111111111111"), "PartNumber", "part_code", null, true, 70 },
                    { new Guid("11111111-0000-0000-0000-000000000008"), new Guid("11111111-1111-1111-1111-111111111111"), "Revision", null, "A", true, 80 },
                    { new Guid("11111111-0000-0000-0000-000000000009"), new Guid("11111111-1111-1111-1111-111111111111"), "Description", "part_name", null, true, 90 },
                    { new Guid("11111111-0000-0000-0000-000000000010"), new Guid("11111111-1111-1111-1111-111111111111"), "Quantity", "qty", null, true, 100 },
                    { new Guid("11111111-0000-0000-0000-000000000011"), new Guid("11111111-1111-1111-1111-111111111111"), "BaseUom", "uom", null, false, 110 },
                    { new Guid("11111111-0000-0000-0000-000000000012"), new Guid("11111111-1111-1111-1111-111111111111"), "MaterialCode", "material_code", null, false, 120 },
                    { new Guid("11111111-0000-0000-0000-000000000013"), new Guid("11111111-1111-1111-1111-111111111111"), "ThicknessMm", "thickness_mm", null, false, 130 },
                    { new Guid("11111111-0000-0000-0000-000000000014"), new Guid("11111111-1111-1111-1111-111111111111"), "ProcessRoute", "process_route", null, false, 140 },
                    { new Guid("11111111-0000-0000-0000-000000000015"), new Guid("11111111-1111-1111-1111-111111111111"), "CutOnly", "cut_only", null, false, 150 },
                    { new Guid("11111111-0000-0000-0000-000000000016"), new Guid("11111111-1111-1111-1111-111111111111"), "WeightKg", "weight_kg", null, false, 160 },
                    { new Guid("11111111-0000-0000-0000-000000000017"), new Guid("11111111-1111-1111-1111-111111111111"), "Notes", "notes", null, false, 170 },
                    { new Guid("22222222-0000-0000-0000-000000000001"), new Guid("22222222-2222-2222-2222-222222222222"), "ProjectCode", null, "PRJ-CONVEYOR-IMPORT", true, 10 },
                    { new Guid("22222222-0000-0000-0000-000000000002"), new Guid("22222222-2222-2222-2222-222222222222"), "ProjectName", null, "Conveyor Template Import", true, 20 },
                    { new Guid("22222222-0000-0000-0000-000000000003"), new Guid("22222222-2222-2222-2222-222222222222"), "ParentItemCode", "ParentItemCode", null, true, 30 },
                    { new Guid("22222222-0000-0000-0000-000000000004"), new Guid("22222222-2222-2222-2222-222222222222"), "ParentItemName", "ParentItemName", null, true, 40 },
                    { new Guid("22222222-0000-0000-0000-000000000005"), new Guid("22222222-2222-2222-2222-222222222222"), "ComponentItemCode", "ComponentItemCode", null, true, 50 },
                    { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("22222222-2222-2222-2222-222222222222"), "ComponentItemName", "ComponentItemName", null, true, 60 },
                    { new Guid("22222222-0000-0000-0000-000000000007"), new Guid("22222222-2222-2222-2222-222222222222"), "PartNumber", "PartNumber", null, true, 70 },
                    { new Guid("22222222-0000-0000-0000-000000000008"), new Guid("22222222-2222-2222-2222-222222222222"), "Revision", null, "A", true, 80 },
                    { new Guid("22222222-0000-0000-0000-000000000009"), new Guid("22222222-2222-2222-2222-222222222222"), "Description", "Description", null, true, 90 },
                    { new Guid("22222222-0000-0000-0000-000000000010"), new Guid("22222222-2222-2222-2222-222222222222"), "Quantity", "Quantity", null, true, 100 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bom_import_template_field_mappings");

            migrationBuilder.DropTable(
                name: "bom_import_templates");
        }
    }
}
