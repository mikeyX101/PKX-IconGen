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

namespace PKXIconGen.Core.Migrations
{
    public partial class MoreSettingPropreties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BlenderOptionalArgs",
                table: "Settings",
                newName: "BlenderOptionalArguments");

            migrationBuilder.AddColumn<byte>(
                name: "IconStyle",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "RenderScale",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconStyle",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "RenderScale",
                table: "Settings");

            migrationBuilder.RenameColumn(
                name: "BlenderOptionalArguments",
                table: "Settings",
                newName: "BlenderOptionalArgs");
        }
    }
}
