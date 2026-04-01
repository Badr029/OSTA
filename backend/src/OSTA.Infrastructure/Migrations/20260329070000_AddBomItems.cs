using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSTA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBomItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bom_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BomHeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ComponentItemMasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Uom = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ItemCategory = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProcurementType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bom_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bom_items_bom_headers_BomHeaderId",
                        column: x => x.BomHeaderId,
                        principalTable: "bom_headers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bom_items_item_masters_ComponentItemMasterId",
                        column: x => x.ComponentItemMasterId,
                        principalTable: "item_masters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bom_items_BomHeaderId_ItemNumber",
                table: "bom_items",
                columns: new[] { "BomHeaderId", "ItemNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bom_items_ComponentItemMasterId",
                table: "bom_items",
                column: "ComponentItemMasterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bom_items");
        }
    }
}
