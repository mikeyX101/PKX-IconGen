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
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using PKXIconGen.Core.Data.Compatibility;

namespace PKXIconGen.Core.Data
{
    [JsonConverter(typeof(ShinyInfoJsonConverter))]
    public class ShinyInfo : IJsonSerializable, IEquatable<ShinyInfo>, ICloneable
    {
        [JsonPropertyName("color1")]
        public ShinyColor? Color1 { get; set; }
        
        [JsonPropertyName("color2")]
        public ShinyColor? Color2 { get; set; }

        [JsonPropertyName("render"), UsedImplicitly, Obsolete("Used for old JSON. Use FaceRender instead.")]
        public RenderData Render
        {
            set => FaceRender = value;
        }
        [JsonPropertyName("face")]
        public RenderData FaceRender { get; private set; }
        
        [JsonPropertyName("box")]
        public BoxInfo BoxRender { get; set; }
        
        public ShinyInfo()
        {
            Color1 = ShinyColor.GetDefaultShinyColor1();
            Color2 = ShinyColor.GetDefaultShinyColor2();
            FaceRender = new RenderData();
            BoxRender = new BoxInfo();
        }
        
        [JsonConstructor]
        public ShinyInfo(ShinyColor? color1, ShinyColor? color2, RenderData faceRender, BoxInfo? boxRender)
        {
            if ((!color1.HasValue || !color2.HasValue) && faceRender.Model is null)
            {
                Color1 = ShinyColor.GetDefaultShinyColor1();
                Color2 = ShinyColor.GetDefaultShinyColor2();
            }
            else
            {
                Color1 = color1;
                Color2 = color2;
            }
            
            FaceRender = faceRender;
            BoxRender = boxRender ?? new BoxInfo();
        }

        public bool Equals(ShinyInfo? other)
        {
            return other is not null &&
                (Color1 is null && other.Color1 is null || Color1 is not null && Color1.Equals(other.Color1)) &&
                (Color2 is null && other.Color2 is null || Color2 is not null && Color2.Equals(other.Color2)) &&
                FaceRender.Equals(other.FaceRender);
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

        public override int GetHashCode() => (Color1, Color2, FaceRender, BoxRender).GetHashCode();

        public object Clone()
        {
            return new ShinyInfo(Color1, Color2, (RenderData)FaceRender.Clone(), (BoxInfo)BoxRender.Clone());
        }
    }
}
