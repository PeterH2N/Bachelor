using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BsCCaseApi.Library.Migrations
{
    /// <inheritdoc />
    public partial class ControllerChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CaseType",
                table: "Cases",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaseType",
                table: "Cases");
        }
    }
}
