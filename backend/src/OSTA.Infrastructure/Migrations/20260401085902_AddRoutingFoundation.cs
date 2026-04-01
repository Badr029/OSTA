using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSTA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutingFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "routing_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemMasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Revision = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routing_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routing_templates_item_masters_ItemMasterId",
                        column: x => x.ItemMasterId,
                        principalTable: "item_masters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "work_centers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HourlyRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_centers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "routing_operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutingTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OperationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OperationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WorkCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SetupTimeMinutes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RunTimeMinutes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    IsQcGate = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routing_operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_routing_operations_routing_templates_RoutingTemplateId",
                        column: x => x.RoutingTemplateId,
                        principalTable: "routing_templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_routing_operations_work_centers_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalTable: "work_centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_routing_operations_RoutingTemplateId_OperationNumber",
                table: "routing_operations",
                columns: new[] { "RoutingTemplateId", "OperationNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routing_operations_WorkCenterId",
                table: "routing_operations",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_routing_templates_ItemMasterId_Code_Revision",
                table: "routing_templates",
                columns: new[] { "ItemMasterId", "Code", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_centers_Code",
                table: "work_centers",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "routing_operations");

            migrationBuilder.DropTable(
                name: "routing_templates");

            migrationBuilder.DropTable(
                name: "work_centers");
        }
    }
}
