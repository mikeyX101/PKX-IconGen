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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PKXIconGen.Core.Data.Blender;

namespace PKXIconGen.Core.Data
{
    /// <summary>
    /// Data for one Pokemon render.
    /// </summary>
    public struct PokemonRenderData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("builtIn")]
        public bool BuiltIn { get; set; }

        [JsonPropertyName("camera")]
        internal Camera Camera { get; set; }
        [JsonPropertyName("lights")]
        internal Light[] Lights { get; set; }

        public PokemonRenderData(string name, string model, bool builtIn)
        {
            Name = name;
            Model = model;
            BuiltIn = builtIn;

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
    }
}
