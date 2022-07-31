#region License
/*  PKX-IconGen.Core - Pokemon Icon Generator for GCN/WII Pokemon games
    Copyright (C) 2021-2022 Samuel Caron/mikeyX#4697

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>. 
*/
#endregion

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PKXIconGen.Core.Migrations
{
    public partial class ObjectShading : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Shiny",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"hue\":1,\"render\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"hue\":1,\"render\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}");

            migrationBuilder.AlterColumn<string>(
                name: "Render",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Shiny",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"hue\":1,\"render\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"hue\":1,\"render\":{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}}");

            migrationBuilder.AlterColumn<string>(
                name: "Render",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"model\":null,\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"focus\":{\"x\":0,\"y\":0,\"z\":0},\"fov\":40,\"is_ortho\":true,\"ortho_scale\":7.31429,\"light\":{\"type\":0,\"strength\":250,\"color\":{\"r\":1,\"g\":1,\"b\":1,\"a\":1},\"distance\":5}},\"secondary_camera\":null,\"removed_objects\":[],\"textures\":[],\"shading\":0,\"bg\":{\"r\":0,\"g\":0,\"b\":0,\"a\":1},\"glow\":{\"r\":1,\"g\":1,\"b\":1,\"a\":0}}");
        }
    }
}
