﻿#region License
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
    public partial class MoreModelChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemovedObjects",
                table: "PokemonRenderData");

            migrationBuilder.AlterColumn<string>(
                name: "Shiny",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"filter\":{\"r\":1,\"g\":1,\"b\":1},\"render\":{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"type\":0,\"strength\":10,\"color\":{\"r\":1,\"g\":1,\"b\":1}}},\"secondary_camera\":null,\"removed_objects\":[]}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"filter\":{\"r\":1,\"g\":1,\"b\":1},\"render\":{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"pos\":{\"x\":0,\"y\":0,\"z\":0},\"rot\":{\"x\":0,\"y\":0,\"z\":0},\"light_type\":0,\"strength\":0,\"color\":{\"r\":0,\"g\":0,\"b\":0}}},\"secondary_camera\":null}}");

            migrationBuilder.AlterColumn<string>(
                name: "Render",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"type\":0,\"strength\":10,\"color\":{\"r\":1,\"g\":1,\"b\":1}}},\"secondary_camera\":null,\"removed_objects\":[]}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"pos\":{\"x\":0,\"y\":0,\"z\":0},\"rot\":{\"x\":0,\"y\":0,\"z\":0},\"light_type\":0,\"strength\":0,\"color\":{\"r\":0,\"g\":0,\"b\":0}}},\"secondary_camera\":null}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Shiny",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"filter\":{\"r\":1,\"g\":1,\"b\":1},\"render\":{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"pos\":{\"x\":0,\"y\":0,\"z\":0},\"rot\":{\"x\":0,\"y\":0,\"z\":0},\"light_type\":0,\"strength\":0,\"color\":{\"r\":0,\"g\":0,\"b\":0}}},\"secondary_camera\":null}}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"filter\":{\"r\":1,\"g\":1,\"b\":1},\"render\":{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"type\":0,\"strength\":10,\"color\":{\"r\":1,\"g\":1,\"b\":1}}},\"secondary_camera\":null,\"removed_objects\":[]}}");

            migrationBuilder.AlterColumn<string>(
                name: "Render",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"pos\":{\"x\":0,\"y\":0,\"z\":0},\"rot\":{\"x\":0,\"y\":0,\"z\":0},\"light_type\":0,\"strength\":0,\"color\":{\"r\":0,\"g\":0,\"b\":0}}},\"secondary_camera\":null}",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40,\"light\":{\"type\":0,\"strength\":10,\"color\":{\"r\":1,\"g\":1,\"b\":1}}},\"secondary_camera\":null,\"removed_objects\":[]}");

            migrationBuilder.AddColumn<string>(
                name: "RemovedObjects",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }
    }
}
