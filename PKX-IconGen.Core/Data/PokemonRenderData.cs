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

using Microsoft.EntityFrameworkCore;
using PKXIconGen.Core.Data.Blender;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using PKXIconGen.Core.Services;

namespace PKXIconGen.Core.Data
{
    [Table("PokemonRenderData"), Index(nameof(ID), IsUnique = true, Name = "IDX_ID")]
    public class PokemonRenderData : IJsonSerializable, IEquatable<PokemonRenderData>
    {
        [Column("ID"), Key, Required, JsonIgnore]
        public uint ID { get; internal set; }

        [Column, JsonPropertyName("name")]
        public string Name { get; set; }
        [Column, JsonPropertyName("output_name")]
        public string? OutputName { get; set; }
        [JsonIgnore]
        public string Output => OutputName?.Length > 0 ? OutputName : Name;

        [Column, JsonIgnore]
        public bool BuiltIn { get; set; }

        [Column, JsonPropertyName("render")]
        public RenderData Render { get; internal set; }
        [Column, JsonPropertyName("shiny")]
        public ShinyInfo Shiny { get; internal set; }

        public PokemonRenderData()
        {
            Name = "";
            OutputName = "";
            BuiltIn = false;

            Render = new RenderData();
            Shiny = new ShinyInfo();
        }
        
        internal PokemonRenderData(string name, string? outputName, bool builtIn, RenderData render, ShinyInfo shiny)
        {
            if (name.Length == 0)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            OutputName = !string.IsNullOrEmpty(outputName) ? outputName : null;
            BuiltIn = builtIn;

            Render = render;
            Shiny = shiny;
        }

        [JsonConstructor]
        public PokemonRenderData(string name, string? outputName, RenderData render, ShinyInfo shiny) 
            : this(name, outputName, false, render, shiny)
        {

        }

        public async Task ModifyAsync(IBlenderRunnerInfo blenderRunnerInfo, CancellationToken token, Action<ReadOnlyMemory<char>>? onOutput = null, Action<PokemonRenderData?>? onFinish = null)
        {
            IBlenderRunner runner = BlenderRunner.BlenderRunners.GetModifyDataRunner(blenderRunnerInfo, this);
            if (onOutput != null)
            {
                runner.OnOutput += new IBlenderRunner.OutDel(onOutput);
            }
            if (onFinish != null)
            {
                runner.OnFinish += new IBlenderRunner.FinishDel(onFinish);
            }
            
            await runner.RunAsync(token);
        }

        public bool Equals(PokemonRenderData? other)
        {
            return other is not null &&
                ID == other.ID &&
                Name == other.Name &&
                BuiltIn == other.BuiltIn &&
                Render.Equals(other.Render) &&
                Shiny.Equals(other.Shiny);
        }
        public override bool Equals(object? obj)
        {
            return obj is PokemonRenderData prd && Equals(prd);
        }

        public static bool operator ==(PokemonRenderData? left, PokemonRenderData? right)
        {
            return left?.Equals(right) ?? left is null && right is null;
        }

        public static bool operator !=(PokemonRenderData? left, PokemonRenderData? right)
        {
            return !(left == right);
        }

        public override int GetHashCode() => 
            (
                ID, 
                Name,
                OutputName,
                BuiltIn,
                Render,
                Shiny
            ).GetHashCode();
    }
}
