using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSTA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBomHeaders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bom_headers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentItemMasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BaseQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Usage = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PlantCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bom_headers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bom_headers_item_masters_ParentItemMasterId",
                        column: x => x.ParentItemMasterId,
                        principalTable: "item_masters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bom_headers_ParentItemMasterId_Revision_Usage_PlantCode",
                table: "bom_headers",
                columns: new[] { "ParentItemMasterId", "Revision", "Usage", "PlantCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bom_headers");
        }
    }
}
