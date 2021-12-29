using Microsoft.EntityFrameworkCore.Migrations;

namespace PKXIconGen.Core.Migrations
{
    public partial class MoreSettingPropreties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BlenderOptionalArgs",
                table: "Settings",
                newName: "BlenderOptionalArguments");

            migrationBuilder.AddColumn<byte>(
                name: "IconStyle",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "RenderScale",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconStyle",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "RenderScale",
                table: "Settings");

            migrationBuilder.RenameColumn(
                name: "BlenderOptionalArguments",
                table: "Settings",
                newName: "BlenderOptionalArgs");
        }
    }
}
