using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoExamenU2.Migrations.LogsDatabase
{
    /// <inheritdoc />
    public partial class addMeesageLogsDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "message",
                schema: "dbo",
                table: "logs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "message",
                schema: "dbo",
                table: "logs");
        }
    }
}
