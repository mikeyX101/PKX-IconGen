using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PKXIconGen.Core.Migrations
{
    public partial class CameraLight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Shiny",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"filter\":{\"r\":1,\"g\":1,\"b\":1},\"render\":{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"pos\":{\"x\":0,\"y\":0,\"z\":0},\"rot\":{\"x\":0,\"y\":0,\"z\":0},\"light_type\":0,\"strength\":0,\"color\":{\"r\":0,\"g\":0,\"b\":0}}},\"secondary_camera\":null}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"filter\":null,\"render\":{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"secondary_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"main_lights\":[],\"secondary_lights\":[]}}");

            migrationBuilder.AlterColumn<string>(
                name: "Render",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"pos\":{\"x\":0,\"y\":0,\"z\":0},\"rot\":{\"x\":0,\"y\":0,\"z\":0},\"light_type\":0,\"strength\":0,\"color\":{\"r\":0,\"g\":0,\"b\":0}}},\"secondary_camera\":null}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"secondary_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"main_lights\":[],\"secondary_lights\":[]}");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "ID",
                keyValue: 1u,
                column: "RenderScale",
                value: (byte)1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Shiny",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"filter\":null,\"render\":{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"secondary_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"main_lights\":[],\"secondary_lights\":[]}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"filter\":{\"r\":1,\"g\":1,\"b\":1},\"render\":{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"pos\":{\"x\":0,\"y\":0,\"z\":0},\"rot\":{\"x\":0,\"y\":0,\"z\":0},\"light_type\":0,\"strength\":0,\"color\":{\"r\":0,\"g\":0,\"b\":0}}},\"secondary_camera\":null}}");

            migrationBuilder.AlterColumn<string>(
                name: "Render",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"secondary_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"main_lights\":[],\"secondary_lights\":[]}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"pos\":{\"x\":0,\"y\":0,\"z\":0},\"rot\":{\"x\":0,\"y\":0,\"z\":0},\"light_type\":0,\"strength\":0,\"color\":{\"r\":0,\"g\":0,\"b\":0}}},\"secondary_camera\":null}");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "ID",
                keyValue: 1u,
                column: "RenderScale",
                value: (byte)0);
        }
    }
}
