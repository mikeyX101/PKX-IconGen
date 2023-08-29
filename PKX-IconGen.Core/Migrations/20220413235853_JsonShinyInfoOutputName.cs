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
    public partial class JsonShinyInfoOutputName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Shiny",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "{\"filter\":null,\"render\":{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"secondary_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"main_lights\":[],\"secondary_lights\":[]}}",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "OutputName",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutputName",
                table: "PokemonRenderData");

            migrationBuilder.AlterColumn<string>(
                name: "Shiny",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "{\"filter\":null,\"render\":{\"model\":\"\",\"animation_pose\":0,\"animation_frame\":0,\"main_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"secondary_camera\":{\"pos\":{\"x\":14,\"y\":-13.5,\"z\":5.5},\"rot\":{\"x\":86.8,\"y\":0,\"z\":54},\"fov\":40},\"main_lights\":[],\"secondary_lights\":[]}}");
        }
    }
}
