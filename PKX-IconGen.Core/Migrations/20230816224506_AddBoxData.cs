using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PKXIconGen.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddBoxData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Render",
                table: "PokemonRenderData",
                newName: "FaceRender");

            migrationBuilder.AlterColumn<string>(
                name: "Shiny",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"color1\":{\"r\":0,\"g\":1,\"b\":2,\"a\":3},\"color2\":{\"r\":127,\"g\":127,\"b\":127,\"a\":127},\"face\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"box\":{\"first\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"second\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"third\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"hue\":1,\"render\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}");

            migrationBuilder.AddColumn<string>(
                name: "BoxRender",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"first\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":0},\"glow\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1}},\"second\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":0},\"glow\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1}},\"third\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":0},\"glow\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1}}}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoxRender",
                table: "PokemonRenderData");

            migrationBuilder.RenameColumn(
                name: "FaceRender",
                table: "PokemonRenderData",
                newName: "Render");

            migrationBuilder.AlterColumn<string>(
                name: "Shiny",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"hue\":1,\"render\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"color1\":{\"r\":0,\"g\":1,\"b\":2,\"a\":3},\"color2\":{\"r\":127,\"g\":127,\"b\":127,\"a\":127},\"face\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"box\":{\"first\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"second\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"third\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}}");
        }
    }
}
