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

namespace PKXIconGen.Core.Data.Blender
{
    /// <summary>
    /// Color reported from Blender, storing each color value in the range [0..1].
    /// </summary>
    public readonly struct Color : IJsonSerializable, IEquatable<Color>
    {
        [JsonPropertyName("r")]
        public readonly float Red { get; init; }
        [JsonPropertyName("g")]
        public readonly float Green { get; init; }
        [JsonPropertyName("b")]
        public readonly float Blue { get; init; }

        [JsonIgnore]
        public readonly byte RValue => (byte)Math.Floor(Utils.ConvertRange(0, 1, 0, 255, Red));
        [JsonIgnore]
        public readonly byte GValue => (byte)Math.Floor(Utils.ConvertRange(0, 1, 0, 255, Green));
        [JsonIgnore]
        public readonly byte BValue => (byte)Math.Floor(Utils.ConvertRange(0, 1, 0, 255, Blue));

        // Floats should be between 0 and 1. Otherwise they will be clamped to that range.
        [UsedImplicitly]
        public Color(float r, float g, float b)
        {
            Red = Math.Clamp(r, 0, 1);
            Green = Math.Clamp(g, 0, 1);
            Blue = Math.Clamp(b, 0, 1);
        }

        public bool Equals(Color other)
        {
            return
                Red == other.Red &&
                Green == other.Green &&
                Blue == other.Blue;
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

        public readonly override int GetHashCode() => (Red, Green, Blue).GetHashCode();

        public uint ToUInt()
        {
            byte red = (byte)Math.Round(Utils.ConvertRange(0, 1, 0, 255, Red));
            byte green = (byte)Math.Round(Utils.ConvertRange(0, 1, 0, 255, Green));
            byte blue = (byte)Math.Round(Utils.ConvertRange(0, 1, 0, 255, Blue));

            return (uint)(0xFF << 24 | red << 16 | green << 8 | blue);
        }

        public static Color FromRgbInt(uint rgb)
        {
            return FromRgbInt(checked((int)(rgb & 0x00FFFFFF)));
        }
        public static Color FromRgbInt(int rgb)
        {
            int red = rgb >> 16 & 0x0000FF;
            int green = rgb >> 8 & 0x0000FF;
            int blue = rgb & 0x0000FF;

            return FromInts(red, green, blue);
        }
        public static Color FromInts(int red, int green, int blue)
        {
            red = Math.Clamp(red, 0, 255);
            green = Math.Clamp(green, 0, 255);
            blue = Math.Clamp(blue, 0, 255);

            float rangeR = Utils.ConvertRange(0, 255, 0, 1, red);
            float rangeG = Utils.ConvertRange(0, 255, 0, 1, green);
            float rangeB = Utils.ConvertRange(0, 255, 0, 1, blue);

            return new Color(rangeR, rangeG, rangeB);
        }

        public static Color GetDefaultColor() => new(255, 255, 255);
    }
}
