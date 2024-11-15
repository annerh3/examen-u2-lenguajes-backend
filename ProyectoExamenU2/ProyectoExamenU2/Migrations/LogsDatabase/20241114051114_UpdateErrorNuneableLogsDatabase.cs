using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoExamenU2.Migrations.LogsDatabase
{
    /// <inheritdoc />
    public partial class UpdateErrorNuneableLogsDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_logs_LogsErrors_log_error_id",
                schema: "dbo",
                table: "logs");

            migrationBuilder.AlterColumn<Guid>(
                name: "log_error_id",
                schema: "dbo",
                table: "logs",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_logs_LogsErrors_log_error_id",
                schema: "dbo",
                table: "logs",
                column: "log_error_id",
                principalTable: "LogsErrors",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_logs_LogsErrors_log_error_id",
                schema: "dbo",
                table: "logs");

            migrationBuilder.AlterColumn<Guid>(
                name: "log_error_id",
                schema: "dbo",
                table: "logs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_logs_LogsErrors_log_error_id",
                schema: "dbo",
                table: "logs",
                column: "log_error_id",
                principalTable: "LogsErrors",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
