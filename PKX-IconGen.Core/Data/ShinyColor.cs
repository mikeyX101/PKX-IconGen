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

namespace PKXIconGen.Core.Data;

public readonly struct ShinyColor : IJsonSerializable, IEquatable<ShinyColor>, ICloneable
{
    [JsonPropertyName("r")]
    public readonly byte R { get; init; }
    [JsonPropertyName("g")]
    public readonly byte G { get; init; }
    [JsonPropertyName("b")]
    public readonly byte B { get; init; }
    [JsonPropertyName("a")]
    public readonly byte A { get; init; }
    
    [JsonIgnore]
    public readonly string DisplayString => $"({R},{G},{B},{A})";

    public ShinyColor(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
    
    public bool Equals(ShinyColor other)
    {
        return R == other.R &&
               G == other.G &&
               B == other.B &&
               A == other.A;
    }

    public override bool Equals(object? obj)
    {
        return obj is ShinyColor shiny && Equals(shiny);
    }

    public static bool operator ==(ShinyColor left, ShinyColor right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ShinyColor left, ShinyColor right)
    {
        return !(left == right);
    }

    public readonly override int GetHashCode() => (R, G, B, A).GetHashCode();

    public object Clone()
    {
        return new ShinyColor(R, G, B, A);
    }

    public override string ToString() => DisplayString;

    public static ShinyColor GetDefaultShinyColor1() => new(0, 1, 2, 3);
    public static ShinyColor GetDefaultShinyColor2() => new(0x7F, 0x7F, 0x7F, 0x7F);
}