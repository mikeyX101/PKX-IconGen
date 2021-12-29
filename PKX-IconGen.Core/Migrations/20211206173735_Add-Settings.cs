using Microsoft.EntityFrameworkCore.Migrations;

namespace PKXIconGen.Core.Migrations
{
    public partial class AddSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    ID = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BlenderPath = table.Column<string>(type: "TEXT", nullable: false),
                    BlenderOptionalArgs = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.ID);
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "ID", "BlenderOptionalArgs", "BlenderPath" },
                values: new object[] { 1u, "", "" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
