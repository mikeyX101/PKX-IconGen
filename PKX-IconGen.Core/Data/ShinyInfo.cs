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
using SixLabors.ImageSharp.PixelFormats;

namespace PKXIconGen.Core.Data
{
    public class ShinyInfo : IJsonSerializable, IEquatable<ShinyInfo>, ICloneable
    {
        [JsonPropertyName("hue")]
        public float? Hue { get; set; }

        [JsonPropertyName("render")]
        public RenderData Render { get; init; }

        public ShinyInfo()
        {
            Hue = 1;
            Render = new RenderData();
        }

        public ShinyInfo(float? hue, RenderData renderData)
        {
            Hue = hue;
            Render = renderData;
        }

        public bool Equals(ShinyInfo? other)
        {
            return other is not null &&
                Hue.Equals(other.Hue) &&
                Render.Equals(other.Render);
        }
        public override bool Equals(object? obj)
        {
            return obj is ShinyInfo shiny && Equals(shiny);
        }

        public static bool operator ==(ShinyInfo? left, ShinyInfo? right)
        {
            return left?.Equals(right) ?? left is null && right is null;
        }

        public static bool operator !=(ShinyInfo left, ShinyInfo right)
        {
            return !(left == right);
        }

        public override int GetHashCode() => (Hue, Render).GetHashCode();

        public object Clone()
        {
            return new ShinyInfo(Hue, (RenderData)Render.Clone());
        }
    }
}
