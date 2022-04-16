using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PKXIconGen.Core.Migrations
{
    public partial class Refact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnimationFrame",
                table: "PokemonRenderData");

            migrationBuilder.DropColumn(
                name: "AnimationPose",
                table: "PokemonRenderData");

            migrationBuilder.DropColumn(
                name: "MainCamera",
                table: "PokemonRenderData");

            migrationBuilder.DropColumn(
                name: "MainLights",
                table: "PokemonRenderData");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "PokemonRenderData");

            migrationBuilder.DropColumn(
                name: "SecondaryCamera",
                table: "PokemonRenderData");

            migrationBuilder.DropColumn(
                name: "SecondaryLights",
                table: "PokemonRenderData");

            migrationBuilder.AlterColumn<string>(
                name: "RemovedObjects",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Render",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"secondary_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"main_lights\":[],\"secondary_lights\":[]}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Render",
                table: "PokemonRenderData");

            migrationBuilder.AlterColumn<string>(
                name: "RemovedObjects",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "[]");

            migrationBuilder.AddColumn<ushort>(
                name: "AnimationFrame",
                table: "PokemonRenderData",
                type: "INTEGER",
                nullable: false,
                defaultValue: (ushort)0);

            migrationBuilder.AddColumn<ushort>(
                name: "AnimationPose",
                table: "PokemonRenderData",
                type: "INTEGER",
                nullable: false,
                defaultValue: (ushort)0);

            migrationBuilder.AddColumn<string>(
                name: "MainCamera",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MainLights",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryCamera",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryLights",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
