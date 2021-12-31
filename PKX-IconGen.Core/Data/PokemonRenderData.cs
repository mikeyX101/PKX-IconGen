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
    [Table("PokemonRenderData"), Index(nameof(InternalID), IsUnique = true, Name = "IDX_ID")]
    public class PokemonRenderData : IJsonSerializable, IEquatable<PokemonRenderData>
    {
        [Column("ID"), Key, JsonIgnore]
        public uint InternalID { get; private set; }

        [Column, JsonPropertyName("name")]
        public string Name { get; set; }
        /// <summary>
        /// Model path. Can contain {{AssetsPath}} to represent the path to extracted assets.
        /// </summary>
        [Column, JsonPropertyName("model")]
        public string Model { get; set; }
        [Column, JsonPropertyName("builtIn")]
        public bool BuiltIn { get; set; }

        [Column, JsonPropertyName("camera")]
        internal Camera Camera { get; private set; }
        [Column, JsonPropertyName("lights")]
        internal Light[] Lights { get; private set; }

        internal PokemonRenderData()
        {
            Name = "";
            Model = "";
            BuiltIn = false;

            Camera = Camera.GetDefaultCamera();
            Lights = Array.Empty<Light>();
        }
        [JsonConstructor]
        internal PokemonRenderData(string name, string model, bool builtIn, Camera camera, Light[] lights)
        {
            Name = name;
            Model = model;
            BuiltIn = builtIn;

            Camera = camera;
            Lights = lights;
        }

        public PokemonRenderData(string name, string model, bool builtIn)
        {
            Name = name;
            Model = model;
            BuiltIn = builtIn;

            Camera = Camera.GetDefaultCamera();
            Lights = Array.Empty<Light>();
        }

        public bool Equals(PokemonRenderData? other)
        {
            return other != null &&
                Name == other.Name &&
                Model == other.Model &&
                BuiltIn == other.BuiltIn &&
                Camera.Equals(other.Camera) &&
                Lights.SequenceEqual(other.Lights);
        }
    }
}
