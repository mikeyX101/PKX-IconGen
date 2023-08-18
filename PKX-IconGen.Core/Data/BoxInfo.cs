#region License
/*  PKX-IconGen.Core - Pokemon Icon Generator for GCN/WII Pokemon games
    Copyright (C) 2021-2023 Samuel Caron/mikeyx

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

namespace PKXIconGen.Core.Data
{
    public enum BoxAnimationFrame : byte
    {
        First = 0,
        Second = 1,
        Third = 2
    }
    
    public class BoxInfo : IJsonSerializable, IEquatable<BoxInfo>, ICloneable
    {
        
        [JsonPropertyName("first")]
        public RenderData First { get; init; }
        
        [JsonPropertyName("second")]
        public RenderData Second { get; init; }
        
        [JsonPropertyName("third")]
        public RenderData Third { get; init; }

        public BoxInfo()
        {
            First = new RenderData();
            Second = new RenderData();
            Third = new RenderData();
        }

        [JsonConstructor]
        public BoxInfo(
            RenderData first,
            RenderData second,
            RenderData third)
        {
            First = first;
            Second = second;
            Third = third;
        }

        public bool Equals(BoxInfo? other)
        {
            return other is not null &&
               First == other.First &&
               Second == other.Second &&
               Third == other.Third;
        }
        public override bool Equals(object? obj)
        {
            return obj is BoxInfo boxInfo && Equals(boxInfo);
        }

        public static bool operator ==(BoxInfo? left, BoxInfo? right)
        {
            return left?.Equals(right) ?? left is null && right is null;
        }

        public static bool operator !=(BoxInfo? left, BoxInfo? right)
        {
            return !(left == right);
        }

        public override int GetHashCode() => (
            First, 
            Second, 
            Third
        ).GetHashCode();

        public object Clone()
        {
            return new BoxInfo(
                (RenderData)First.Clone(),
                (RenderData)Second.Clone(),
                (RenderData)Third.Clone()
            );
        }
    }
}
