using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSTA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutionSourceTraceability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceItemMasterId",
                table: "parts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceBomHeaderId",
                table: "finished_goods",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceItemMasterId",
                table: "finished_goods",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceBomItemId",
                table: "assemblies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceComponentItemMasterId",
                table: "assemblies",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_parts_SourceItemMasterId",
                table: "parts",
                column: "SourceItemMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_finished_goods_SourceBomHeaderId",
                table: "finished_goods",
                column: "SourceBomHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_finished_goods_SourceItemMasterId",
                table: "finished_goods",
                column: "SourceItemMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_assemblies_SourceBomItemId",
                table: "assemblies",
                column: "SourceBomItemId");

            migrationBuilder.CreateIndex(
                name: "IX_assemblies_SourceComponentItemMasterId",
                table: "assemblies",
                column: "SourceComponentItemMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_assemblies_bom_items_SourceBomItemId",
                table: "assemblies",
                column: "SourceBomItemId",
                principalTable: "bom_items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_assemblies_item_masters_SourceComponentItemMasterId",
                table: "assemblies",
                column: "SourceComponentItemMasterId",
                principalTable: "item_masters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_finished_goods_bom_headers_SourceBomHeaderId",
                table: "finished_goods",
                column: "SourceBomHeaderId",
                principalTable: "bom_headers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_finished_goods_item_masters_SourceItemMasterId",
                table: "finished_goods",
                column: "SourceItemMasterId",
                principalTable: "item_masters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_parts_item_masters_SourceItemMasterId",
                table: "parts",
                column: "SourceItemMasterId",
                principalTable: "item_masters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_assemblies_bom_items_SourceBomItemId",
                table: "assemblies");

            migrationBuilder.DropForeignKey(
                name: "FK_assemblies_item_masters_SourceComponentItemMasterId",
                table: "assemblies");

            migrationBuilder.DropForeignKey(
                name: "FK_finished_goods_bom_headers_SourceBomHeaderId",
                table: "finished_goods");

            migrationBuilder.DropForeignKey(
                name: "FK_finished_goods_item_masters_SourceItemMasterId",
                table: "finished_goods");

            migrationBuilder.DropForeignKey(
                name: "FK_parts_item_masters_SourceItemMasterId",
                table: "parts");

            migrationBuilder.DropIndex(
                name: "IX_parts_SourceItemMasterId",
                table: "parts");

            migrationBuilder.DropIndex(
                name: "IX_finished_goods_SourceBomHeaderId",
                table: "finished_goods");

            migrationBuilder.DropIndex(
                name: "IX_finished_goods_SourceItemMasterId",
                table: "finished_goods");

            migrationBuilder.DropIndex(
                name: "IX_assemblies_SourceBomItemId",
                table: "assemblies");

            migrationBuilder.DropIndex(
                name: "IX_assemblies_SourceComponentItemMasterId",
                table: "assemblies");

            migrationBuilder.DropColumn(
                name: "SourceItemMasterId",
                table: "parts");

            migrationBuilder.DropColumn(
                name: "SourceBomHeaderId",
                table: "finished_goods");

            migrationBuilder.DropColumn(
                name: "SourceItemMasterId",
                table: "finished_goods");

            migrationBuilder.DropColumn(
                name: "SourceBomItemId",
                table: "assemblies");

            migrationBuilder.DropColumn(
                name: "SourceComponentItemMasterId",
                table: "assemblies");
        }
    }
}
