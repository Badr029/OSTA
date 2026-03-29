using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OSTA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBomImportTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bom_import_batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    ImportedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    SuccessfulRows = table.Column<int>(type: "integer", nullable: false),
                    FailedRows = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bom_import_batches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bom_import_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BOMImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: false),
                    ProjectCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProjectName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FinishedGoodCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FinishedGoodName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AssemblyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssemblyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PartNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Revision = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bom_import_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bom_import_lines_bom_import_batches_BOMImportBatchId",
                        column: x => x.BOMImportBatchId,
                        principalTable: "bom_import_batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bom_import_lines_BOMImportBatchId_RowNumber",
                table: "bom_import_lines",
                columns: new[] { "BOMImportBatchId", "RowNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bom_import_lines");

            migrationBuilder.DropTable(
                name: "bom_import_batches");
        }
    }
}
