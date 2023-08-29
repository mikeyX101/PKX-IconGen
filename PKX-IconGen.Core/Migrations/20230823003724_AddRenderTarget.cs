using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PKXIconGen.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddRenderTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "RenderTarget",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0b11);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "ID",
                keyValue: 1u,
                column: "RenderTarget",
                value: (byte)0b11);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RenderTarget",
                table: "Settings");
        }
    }
}
