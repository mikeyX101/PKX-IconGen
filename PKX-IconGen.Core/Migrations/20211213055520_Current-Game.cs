using Microsoft.EntityFrameworkCore.Migrations;

namespace PKXIconGen.Migrations
{
    public partial class CurrentGame : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IconStyle",
                table: "Settings",
                newName: "CurrentGame");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CurrentGame",
                table: "Settings",
                newName: "IconStyle");
        }
    }
}
