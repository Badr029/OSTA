using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSTA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddItemMaterialRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "item_material_requirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemMasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequiredQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Uom = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ThicknessMm = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    LengthMm = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    WidthMm = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    WeightKg = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_material_requirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_item_material_requirements_item_masters_ItemMasterId",
                        column: x => x.ItemMasterId,
                        principalTable: "item_masters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_item_material_requirements_ItemMasterId",
                table: "item_material_requirements",
                column: "ItemMasterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item_material_requirements");
        }
    }
}
