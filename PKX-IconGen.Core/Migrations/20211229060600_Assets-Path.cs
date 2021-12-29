using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PKXIconGen.Migrations
{
    public partial class AssetsPath : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssetsPath",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "ID",
                keyValue: 1u,
                column: "AssetsPath",
                value: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetsPath",
                table: "Settings");
        }
    }
}
