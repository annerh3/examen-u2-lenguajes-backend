using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoExamenU2.Migrations.LogsDatabase
{
    /// <inheritdoc />
    public partial class INITLogsDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "LogsDetails",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tableAfected = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    row_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    change_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    old_values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    new_values = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsDetails", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "LogsErrors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    error_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    error_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    stack_trace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    time_stamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsErrors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "logs",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    log_detail_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    log_error_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_logs_LogsDetails_log_detail_id",
                        column: x => x.log_detail_id,
                        principalTable: "LogsDetails",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_logs_LogsErrors_log_error_id",
                        column: x => x.log_error_id,
                        principalTable: "LogsErrors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_logs_log_detail_id",
                schema: "dbo",
                table: "logs",
                column: "log_detail_id");

            migrationBuilder.CreateIndex(
                name: "IX_logs_log_error_id",
                schema: "dbo",
                table: "logs",
                column: "log_error_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "logs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LogsDetails");

            migrationBuilder.DropTable(
                name: "LogsErrors");
        }
    }
}
