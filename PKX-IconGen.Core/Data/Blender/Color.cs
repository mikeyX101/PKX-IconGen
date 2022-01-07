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

namespace PKXIconGen.Core.Data.Blender
{
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

        // Float should be between 0 and 1. Otherwise they will be clamped to that range.
        public Color(float red, float green, float blue)
        {
            Red = Math.Clamp(red, 0, 1);
            Green = Math.Clamp(green, 0, 1);
            Blue = Math.Clamp(blue, 0, 1);
        }

        public bool Equals(Color other)
        {
            return
                Red == other.Red &&
                Green == other.Green &&
                Blue == other.Blue;
        }

		//TODO Double check bit shifts
        public static Color FromRgbInt(int rgb)
        {
            int red = rgb >> 8 & 0x0000FF;
            int green = rgb >> 16 & 0x0000FF;
            int blue = rgb >> 24 & 0x0000FF;

            return FromInts(red, green, blue);
        }
        public static Color FromArgbInt(uint argb)
        {
            // No need
            //uint alpha = argb >> 8 & 0x000000FF;

            uint red = argb >> 16 & 0x000000FF;
            uint green = argb >> 24 & 0x000000FF;
            uint blue = argb >> 32 & 0x000000FF;

            return FromUInts(red, green, blue);
        }
        public static Color FromInts(int red, int green, int blue)
        {
            float rangeR = Utils.ConvertRange(0, 255, 0, 1, red);
            float rangeG = Utils.ConvertRange(0, 255, 0, 1, green);
            float rangeB = Utils.ConvertRange(0, 255, 0, 1, blue);

            return new Color(rangeR, rangeG, rangeB);
        }
        public static Color FromUInts(uint red, uint green, uint blue)
        {
            float rangeR = Utils.ConvertRange(0, 255, 0, 1, red);
            float rangeG = Utils.ConvertRange(0, 255, 0, 1, green);
            float rangeB = Utils.ConvertRange(0, 255, 0, 1, blue);

            return new Color(rangeR, rangeG, rangeB);
        }
    }
}
