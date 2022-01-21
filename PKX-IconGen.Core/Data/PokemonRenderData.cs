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

        [Column, JsonPropertyName("animation_pose")]
        public ushort AnimationPose { get; internal set; }
        [Column, JsonPropertyName("animation_frame")]
        public ushort AnimationFrame { get; internal set; }

        [Column, JsonPropertyName("shiny")]
        public ShinyInfo Shiny { get; internal set; }

        [Column, JsonPropertyName("main_camera")]
        public Camera MainCamera { get; internal set; }
        [Column, JsonPropertyName("secondary_camera")]
        public Camera? SecondaryCamera { get; internal set; }
        [Column, JsonPropertyName("main_lights")]
        public Light[] MainLights { get; internal set; }
        [Column, JsonPropertyName("secondary_lights")]
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
        
        internal PokemonRenderData(string name, string model, bool builtIn, ushort animationPose, ushort animationFrame, ShinyInfo shiny, Camera mainCamera, Light[] mainLights, Camera? secondaryCamera, Light[] secondaryLights)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Model = model;
            BuiltIn = builtIn;

            AnimationPose = animationPose;
            AnimationFrame = animationFrame;

            Shiny = shiny;

            MainCamera = mainCamera;
            SecondaryCamera = secondaryCamera;
            MainLights = mainLights;
            SecondaryLights = secondaryLights;
        }

        [JsonConstructor]
        public PokemonRenderData(string name, string model, ushort animationPose, ushort animationFrame, ShinyInfo shiny, Camera mainCamera, Light[] mainLights, Camera? secondaryCamera, Light[] secondaryLights) 
            : this(name, model, false, animationPose, animationFrame, shiny, mainCamera, mainLights, secondaryCamera, secondaryLights)
        {

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
