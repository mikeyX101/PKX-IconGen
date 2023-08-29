using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PKXIconGen.Core.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToPRDModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Shiny",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"color1\":{\"r\":0,\"g\":1,\"b\":2,\"a\":3},\"color2\":{\"r\":127,\"g\":127,\"b\":127,\"a\":127},\"model\":\"null\",\"face\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"box\":{\"first\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"second\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"third\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"color1\":{\"r\":0,\"g\":1,\"b\":2,\"a\":3},\"color2\":{\"r\":127,\"g\":127,\"b\":127,\"a\":127},\"face\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"box\":{\"first\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"second\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"third\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}}");

            migrationBuilder.AlterColumn<string>(
                name: "FaceRender",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}");

            migrationBuilder.AlterColumn<string>(
                name: "BoxRender",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"first\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"second\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"third\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"first\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"second\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"third\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE PokemonRenderData
                    SET Model = FaceRender -> 'model';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE PokemonRenderData
                    SET FaceRender = json_set(FaceRender, '$.model', Model);
            ");
            
            migrationBuilder.DropColumn(
                name: "Model",
                table: "PokemonRenderData");

            migrationBuilder.AlterColumn<string>(
                name: "Shiny",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"color1\":{\"r\":0,\"g\":1,\"b\":2,\"a\":3},\"color2\":{\"r\":127,\"g\":127,\"b\":127,\"a\":127},\"face\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"box\":{\"first\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"second\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"third\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"color1\":{\"r\":0,\"g\":1,\"b\":2,\"a\":3},\"color2\":{\"r\":127,\"g\":127,\"b\":127,\"a\":127},\"model\":\"null\",\"face\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"box\":{\"first\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"second\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"third\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}}");

            migrationBuilder.AlterColumn<string>(
                name: "FaceRender",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}");

            migrationBuilder.AlterColumn<string>(
                name: "BoxRender",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"first\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"second\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"third\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"first\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"second\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}},\"third\":{\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":31.51,\"y\":-37.49,\"z\":33.89},\"focus\":{\"x\":0,\"y\":-1.75,\"z\":6.8},\"fov\":36,\"is_ortho\":true,\"ortho_scale\":36.86,\"light\":{\"type\":3,\"strength\":650,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":13}},\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}");
        }
    }
}
