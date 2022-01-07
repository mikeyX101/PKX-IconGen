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
        public string Name { get; internal set; }
        /// <summary>
        /// Model path. Can contain {{AssetsPath}} to represent the path to extracted assets.
        /// </summary>
        [Column, JsonPropertyName("model")]
        public string Model { get; internal set; }
        [Column, JsonPropertyName("builtIn")]
        public bool BuiltIn { get; internal set; }

        [Column, JsonPropertyName("animationPose")]
        public ushort AnimationPose { get; internal set; }
        [Column, JsonPropertyName("animationFrame")]
        public ushort AnimationFrame { get; internal set; }

        [Column, JsonPropertyName("shiny")]
        public ShinyInfo Shiny { get; internal set; }

        [Column, JsonPropertyName("mainCamera")]
        public Camera MainCamera { get; internal set; }
        [Column, JsonPropertyName("secondaryCamera")]
        public Camera? SecondaryCamera { get; internal set; }
        [Column, JsonPropertyName("mainLights")]
        public Light[] MainLights { get; internal set; }
        [Column, JsonPropertyName("secondaryLights")]
        public Light[] SecondaryLights { get; internal set; }

        internal PokemonRenderData()
        {
            Name = "";
            Model = "";
            BuiltIn = false;

            MainCamera = Camera.GetDefaultCamera();
            SecondaryCamera = null;
            MainLights = Array.Empty<Light>();
            SecondaryLights = Array.Empty<Light>();
        }
        [JsonConstructor]
        internal PokemonRenderData(string name, string model, bool builtIn, ShinyInfo shiny, Camera rightCamera, Light[] rightLights, Camera? leftCamera, Light[] leftLights)
        {
            Name = name;
            Model = model;
            BuiltIn = builtIn;

            Shiny = shiny;

            MainCamera = rightCamera;
            SecondaryCamera = leftCamera;
            MainLights = rightLights;
            SecondaryLights = leftLights;
        }

        public PokemonRenderData(string name, string model)
        {
            Name = name;
            Model = model;
            BuiltIn = false;

            MainCamera = Camera.GetDefaultCamera();
            SecondaryCamera = null;
            MainLights = Array.Empty<Light>();
            SecondaryLights = Array.Empty<Light>();
        }

        public bool Equals(PokemonRenderData? other)
        {
            return other != null && 
                InternalID == other.InternalID &&
                Name == other.Name &&
                Model == other.Model &&
                BuiltIn == other.BuiltIn &&
                AnimationPose == other.AnimationPose &&
                AnimationFrame == other.AnimationFrame &&
                MainCamera.Equals(other.MainCamera) &&
                SecondaryCamera.Equals(other.SecondaryCamera) &&
                MainLights.SequenceEqual(other.MainLights) &&
                SecondaryLights.SequenceEqual(other.SecondaryLights);
        }
    }
}
