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
    public partial class MoreBlenderData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Lights",
                table: "PokemonRenderData",
                newName: "Shiny");

            migrationBuilder.RenameColumn(
                name: "Camera",
                table: "PokemonRenderData",
                newName: "RightLights");

            migrationBuilder.AddColumn<string>(
                name: "LeftCamera",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeftLights",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RightCamera",
                table: "PokemonRenderData",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeftCamera",
                table: "PokemonRenderData");

            migrationBuilder.DropColumn(
                name: "LeftLights",
                table: "PokemonRenderData");

            migrationBuilder.DropColumn(
                name: "RightCamera",
                table: "PokemonRenderData");

            migrationBuilder.RenameColumn(
                name: "Shiny",
                table: "PokemonRenderData",
                newName: "Lights");

            migrationBuilder.RenameColumn(
                name: "RightLights",
                table: "PokemonRenderData",
                newName: "Camera");
        }
    }
}
