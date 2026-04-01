using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSTA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "work_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinishedGoodId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssemblyId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PlannedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CompletedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ReleasedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_orders_assemblies_AssemblyId",
                        column: x => x.AssemblyId,
                        principalTable: "assemblies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_orders_finished_goods_FinishedGoodId",
                        column: x => x.FinishedGoodId,
                        principalTable: "finished_goods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_orders_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "work_order_operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoutingOperationId = table.Column<Guid>(type: "uuid", nullable: true),
                    OperationNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OperationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OperationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WorkCenterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PlannedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CompletedQuantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    IsQcGate = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_order_operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_order_operations_routing_operations_RoutingOperationId",
                        column: x => x.RoutingOperationId,
                        principalTable: "routing_operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_work_order_operations_work_centers_WorkCenterId",
                        column: x => x.WorkCenterId,
                        principalTable: "work_centers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_order_operations_work_orders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "work_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_work_order_operations_RoutingOperationId",
                table: "work_order_operations",
                column: "RoutingOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_operations_WorkCenterId",
                table: "work_order_operations",
                column: "WorkCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_work_order_operations_WorkOrderId_OperationNumber",
                table: "work_order_operations",
                columns: new[] { "WorkOrderId", "OperationNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_AssemblyId",
                table: "work_orders",
                column: "AssemblyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_FinishedGoodId",
                table: "work_orders",
                column: "FinishedGoodId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_ProjectId",
                table: "work_orders",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_WorkOrderNumber",
                table: "work_orders",
                column: "WorkOrderNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "work_order_operations");

            migrationBuilder.DropTable(
                name: "work_orders");
        }
    }
}
