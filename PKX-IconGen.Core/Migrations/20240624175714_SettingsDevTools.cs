using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PKXIconGen.Core.Migrations
{
    /// <inheritdoc />
    public partial class SettingsDevTools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DevTools",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "ID",
                keyValue: 1u,
                columns: new[] { "DevTools", "OutputNameForTarget" },
                values: new object[] { false, (byte)1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DevTools",
                table: "Settings");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "ID",
                keyValue: 1u,
                column: "OutputNameForTarget",
                value: (byte)0);
        }
    }
}
