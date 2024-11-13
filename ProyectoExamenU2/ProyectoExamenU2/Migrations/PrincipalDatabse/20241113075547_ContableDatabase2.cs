using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoExamenU2.Migrations.PrincipalDatabse
{
    /// <inheritdoc />
    public partial class ContableDatabase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_Active",
                schema: "dbo",
                table: "account_catalog",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_Active",
                schema: "dbo",
                table: "account_catalog");
        }
    }
}
