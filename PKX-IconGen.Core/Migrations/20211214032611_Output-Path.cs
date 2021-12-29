using Microsoft.EntityFrameworkCore.Migrations;

namespace PKXIconGen.Migrations
{
    public partial class OutputPath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OutputPath",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "ID",
                keyValue: 1u,
                column: "OutputPath",
                value: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutputPath",
                table: "Settings");
        }
    }
}
