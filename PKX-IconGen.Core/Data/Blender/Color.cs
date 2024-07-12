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
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// We need to let EF init properties

namespace PKXIconGen.Core.Data.Blender;

/// <summary>
/// Color reported from Blender, storing each color value in the range [0..1].
/// </summary>
public readonly struct Color : IJsonSerializable, IEquatable<Color>
{
    [JsonPropertyName("r")]
    public float Red { get; init; }
    [JsonPropertyName("g")]
    public float Green { get; init; }
    [JsonPropertyName("b")]
    public float Blue { get; init; }
    [JsonPropertyName("a")]
    public float Alpha { get; init; }

    [JsonIgnore]
    public byte RValue => (byte)Math.Round(Utils.ConvertRange(0, 1, 0, 255, Red));
    [JsonIgnore]
    public byte GValue => (byte)Math.Round(Utils.ConvertRange(0, 1, 0, 255, Green));
    [JsonIgnore]
    public byte BValue => (byte)Math.Round(Utils.ConvertRange(0, 1, 0, 255, Blue));
    [JsonIgnore]
    public byte AValue => (byte)Math.Round(Utils.ConvertRange(0, 1, 0, 255, Alpha));

    /// Floats should be between 0 and 1. Otherwise they will be clamped to that range.
    public Color(float r, float g, float b, float a)
    {
        Red = Math.Clamp(r, 0, 1);
        Green = Math.Clamp(g, 0, 1);
        Blue = Math.Clamp(b, 0, 1);
        Alpha = Math.Clamp(a, 0, 1);
    }

    public bool Equals(Color other)
    {
        return
            Math.Abs(Red - other.Red) < 0.000000001 &&
            Math.Abs(Green - other.Green) < 0.000000001 &&
            Math.Abs(Blue - other.Blue) < 0.000000001 &&
            Math.Abs(Alpha - other.Alpha) < 0.000000001;
    }
    public override bool Equals(object? obj)
    {
        return obj is Color color && Equals(color);
    }

    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Color left, Color right)
    {
        return !(left == right);
    }

    public override int GetHashCode() => (Red, Green, Blue, Alpha).GetHashCode();

    public uint ToRgbaUInt() => (uint)(RValue << 24 | GValue << 16 | BValue << 8 | AValue);
    public uint ToArgbUInt() => (uint)(AValue << 24 | RValue << 16 | GValue << 8 | BValue);

    public static Color FromRgbaUInt(uint rgba)
    {
        byte red = (byte)(rgba >> 24 & 0x000000FF);
        byte green = (byte)(rgba >> 16 & 0x000000FF);
        byte blue = (byte)(rgba >> 8 & 0x000000FF);
        byte alpha = (byte)(rgba & 0x000000FF);

        return FromBytes(red, green, blue, alpha);
    }
    public static Color FromArgbUInt(uint argb)
    {
        byte alpha = (byte)(argb >> 24 & 0x000000FF);
        byte red = (byte)(argb >> 16 & 0x000000FF);
        byte green = (byte)(argb >> 8 & 0x000000FF);
        byte blue = (byte)(argb & 0x000000FF);

        return FromBytes(red, green, blue, alpha);
    }
    public static Color FromBytes(byte red, byte green, byte blue, byte alpha)
    {
        float rangeR = Utils.ConvertRange(0, 255, 0, 1, red);
        float rangeG = Utils.ConvertRange(0, 255, 0, 1, green);
        float rangeB = Utils.ConvertRange(0, 255, 0, 1, blue);
        float rangeA = Utils.ConvertRange(0, 255, 0, 1, alpha);

        return new Color(rangeR, rangeG, rangeB, rangeA);
    }

    public static Color GetDefaultColor() => new(1, 1, 1, 1);
}