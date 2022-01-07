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

using PKXIconGen.Core.Data.Blender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PKXIconGen.Core.Data
{
    public readonly struct ShinyInfo : IJsonSerializable, IEquatable<ShinyInfo>
    {
        [JsonPropertyName("filter")]
        public Color? Filter { get; init; }

        [JsonPropertyName("altModel")]
        public string? AltModel { get; init; }

        [JsonPropertyName("animationPose")]
        public ushort AnimationPose { get; init; }
        [JsonPropertyName("animationFrame")]
        public ushort AnimationFrame { get; init; }

        public ShinyInfo(Color color, ushort animationNumber, ushort animationFrame)
        {
            Filter = color;
            AltModel = null;

            AnimationPose = animationNumber;
            AnimationFrame = animationFrame;
        }

        public ShinyInfo(string altModel, ushort animationNumber, ushort animationFrame)
        {
            Filter = null;
            AltModel = altModel;

            AnimationPose = animationNumber;
            AnimationFrame = animationFrame;
        }

        public bool Equals(ShinyInfo other)
        {
            return Filter.Equals(other.Filter) &&
                (
                    (AltModel == null && other.AltModel == null) || 
                    (
                        AltModel != null && other.AltModel != null &&
                        AltModel.Equals(other.AltModel)
                    )
                ) &&
                AnimationPose == other.AnimationPose &&
                AnimationFrame == other.AnimationFrame;
        }
    }
}
