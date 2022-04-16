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

namespace PKXIconGen.Core.Data
{
    [Table("PokemonRenderData"), Index(nameof(ID), IsUnique = true, Name = "IDX_ID")]
    public class PokemonRenderData : IJsonSerializable, IEquatable<PokemonRenderData>
    {
        [Column("ID"), Key, Required, JsonIgnore]
        public uint ID { get; internal set; }

        [Column, JsonPropertyName("name")]
        public string Name { get; internal set; }
        [Column, JsonPropertyName("output_name")]
        public string? OutputName { get; internal set; }
        [Column, JsonPropertyName("builtIn")]
        public bool BuiltIn { get; internal set; }

        [Column, JsonPropertyName("render")]
        public RenderData Render { get; internal set; }
        [Column, JsonPropertyName("shiny")]
        public ShinyInfo Shiny { get; internal set; }

        [Column, JsonPropertyName("removed_objects")]
        public string[] RemovedObjects { get; internal set; }

        public PokemonRenderData()
        {
            Name = "";
            OutputName = "";
            BuiltIn = false;

            Render = new();
            Shiny = new();

            RemovedObjects = Array.Empty<string>();
        }
        
        internal PokemonRenderData(string name, string? outputName, bool builtIn, RenderData render, ShinyInfo shiny, string[] removedObjects)
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

            RemovedObjects = removedObjects;
        }

        [JsonConstructor]
        public PokemonRenderData(string name, string? outputName, RenderData render, ShinyInfo shiny, string[] removedObjects) 
            : this(name, outputName, false, render, shiny, removedObjects)
        {

        }

        public bool Equals(PokemonRenderData? other)
        {
            return other is not null && 
                ID == other.ID &&
                Name == other.Name &&
                BuiltIn == other.BuiltIn &&
                Render.Equals(other.Render) &&
                Shiny.Equals(other.Shiny) &&
                RemovedObjects.SequenceEqual(other.RemovedObjects);
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
                BuiltIn,
                Render,
                Shiny
            ).GetHashCode();
    }
}
