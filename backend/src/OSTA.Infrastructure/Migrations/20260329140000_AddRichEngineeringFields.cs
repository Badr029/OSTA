using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSTA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRichEngineeringFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DrawingNumber",
                table: "item_masters",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinishCode",
                table: "item_masters",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HeightMm",
                table: "item_masters",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LengthMm",
                table: "item_masters",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialCode",
                table: "item_masters",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "item_masters",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Specification",
                table: "item_masters",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ThicknessMm",
                table: "item_masters",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                table: "item_masters",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WidthMm",
                table: "item_masters",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CutOnly",
                table: "bom_items",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBulk",
                table: "bom_items",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPhantom",
                table: "bom_items",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineNotes",
                table: "bom_items",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionText",
                table: "bom_items",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessRouteCode",
                table: "bom_items",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ScrapPercent",
                table: "bom_items",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CutOnly",
                table: "bom_import_lines",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialCode",
                table: "bom_import_lines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "bom_import_lines",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessRouteCode",
                table: "bom_import_lines",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ThicknessMm",
                table: "bom_import_lines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightKg",
                table: "bom_import_lines",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DrawingNumber",
                table: "item_masters");

            migrationBuilder.DropColumn(
                name: "FinishCode",
                table: "item_masters");

            migrationBuilder.DropColumn(
                name: "HeightMm",
                table: "item_masters");

            migrationBuilder.DropColumn(
                name: "LengthMm",
                table: "item_masters");

            migrationBuilder.DropColumn(
                name: "MaterialCode",
                table: "item_masters");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "item_masters");

            migrationBuilder.DropColumn(
                name: "Specification",
                table: "item_masters");

            migrationBuilder.DropColumn(
                name: "ThicknessMm",
                table: "item_masters");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "item_masters");

            migrationBuilder.DropColumn(
                name: "WidthMm",
                table: "item_masters");

            migrationBuilder.DropColumn(
                name: "CutOnly",
                table: "bom_items");

            migrationBuilder.DropColumn(
                name: "IsBulk",
                table: "bom_items");

            migrationBuilder.DropColumn(
                name: "IsPhantom",
                table: "bom_items");

            migrationBuilder.DropColumn(
                name: "LineNotes",
                table: "bom_items");

            migrationBuilder.DropColumn(
                name: "PositionText",
                table: "bom_items");

            migrationBuilder.DropColumn(
                name: "ProcessRouteCode",
                table: "bom_items");

            migrationBuilder.DropColumn(
                name: "ScrapPercent",
                table: "bom_items");

            migrationBuilder.DropColumn(
                name: "CutOnly",
                table: "bom_import_lines");

            migrationBuilder.DropColumn(
                name: "MaterialCode",
                table: "bom_import_lines");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "bom_import_lines");

            migrationBuilder.DropColumn(
                name: "ProcessRouteCode",
                table: "bom_import_lines");

            migrationBuilder.DropColumn(
                name: "ThicknessMm",
                table: "bom_import_lines");

            migrationBuilder.DropColumn(
                name: "WeightKg",
                table: "bom_import_lines");
        }
    }
}
