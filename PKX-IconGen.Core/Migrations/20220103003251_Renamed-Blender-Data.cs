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
    public partial class RenamedBlenderData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RightLights",
                table: "PokemonRenderData",
                newName: "SecondaryLights");

            migrationBuilder.RenameColumn(
                name: "RightCamera",
                table: "PokemonRenderData",
                newName: "MainLights");

            migrationBuilder.RenameColumn(
                name: "LeftLights",
                table: "PokemonRenderData",
                newName: "MainCamera");

            migrationBuilder.RenameColumn(
                name: "LeftCamera",
                table: "PokemonRenderData",
                newName: "SecondaryCamera");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SecondaryLights",
                table: "PokemonRenderData",
                newName: "RightLights");

            migrationBuilder.RenameColumn(
                name: "SecondaryCamera",
                table: "PokemonRenderData",
                newName: "LeftCamera");

            migrationBuilder.RenameColumn(
                name: "MainLights",
                table: "PokemonRenderData",
                newName: "RightCamera");

            migrationBuilder.RenameColumn(
                name: "MainCamera",
                table: "PokemonRenderData",
                newName: "LeftLights");
        }
    }
}
