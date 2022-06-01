#region License
/*  PKX-IconGen.Core.Tests - PKX-IconGen Core Unit Tests
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

using NUnit.Framework;
using PKXIconGen.Core.Data.Blender;
using System;

namespace PKXIconGen.Core.Tests.Data
{
    public class ColorTests
    {
        private static void ColorShouldEqualValues(ref Color color, float expectedR, float expectedG, float expectedB, string assertMessage)
        {
            Assert.AreEqual(expectedR, color.Red, assertMessage + ", Red");
            Assert.AreEqual(expectedG, color.Green, assertMessage + ", Green");
            Assert.AreEqual(expectedB, color.Blue, assertMessage + ", Blue");
        }

        [Test, Order(1)]
        public void ColorClampValues()
        {
            Color minR = new(-0.001f, 0, 0);
            ColorShouldEqualValues(ref minR, 0, 0, 0, nameof(minR));
            Color minG = new(0, -0.001f, 0);
            ColorShouldEqualValues(ref minG, 0, 0, 0, nameof(minG));
            Color minB = new(0, 0, -0.001f);
            ColorShouldEqualValues(ref minB, 0, 0, 0, nameof(minB));

            Color maxR = new(1.001f, 0, 0);
            ColorShouldEqualValues(ref maxR, 1, 0, 0, nameof(minR));
            Color maxG = new(0, 1.001f, 0);
            ColorShouldEqualValues(ref maxG, 0, 1, 0, nameof(minG));
            Color maxB = new(0, 0, 1.001f); 
            ColorShouldEqualValues(ref maxB, 0, 0, 1, nameof(minB));

            Color teal = new(0.40000004f, 1, 0.8000001f);
            ColorShouldEqualValues(ref teal, 0.40000004f, 1, 0.8000001f, nameof(teal));
        }

        [Test, Order(2)]
        public void ColorFromInts()
        {
            Color white = Color.FromInts(0, 0, 0);
            ColorShouldEqualValues(ref white, 0, 0, 0, nameof(white));
            Color black = Color.FromInts(255, 255, 255);
            ColorShouldEqualValues(ref black, 1, 1, 1, nameof(black));
            Color red = Color.FromInts(255, 0, 0);
            ColorShouldEqualValues(ref red, 1, 0, 0, nameof(red));
            Color green = Color.FromInts(0, 255, 0);
            ColorShouldEqualValues(ref green, 0, 1, 0, nameof(green));
            Color blue = Color.FromInts(0, 0, 255);
            ColorShouldEqualValues(ref blue, 0, 0, 1, nameof(blue));

            Color underMin = Color.FromInts(-1, -1, -1);
            ColorShouldEqualValues(ref underMin, 0, 0, 0, nameof(underMin));
            Color overMax = Color.FromInts(256, 256, 256);
            ColorShouldEqualValues(ref overMax, 1, 1, 1, nameof(overMax));

            Color teal = Color.FromInts(102, 255, 204);
            ColorShouldEqualValues(ref teal, 0.40000004f, 1, 0.8000001f, nameof(teal));
        }

        [Test, Order(3)]
        public void ColorFromRgbInt()
        {
            Color white = Color.FromRgbInt(0x000000);
            ColorShouldEqualValues(ref white, 0, 0, 0, nameof(white));
            Color black = Color.FromRgbInt(0xFFFFFF);
            ColorShouldEqualValues(ref black, 1, 1, 1, nameof(black));
            Color red = Color.FromRgbInt(0xFF0000);
            ColorShouldEqualValues(ref red, 1, 0, 0, nameof(red));
            Color green = Color.FromRgbInt(0x00FF00);
            ColorShouldEqualValues(ref green, 0, 1, 0, nameof(green));
            Color blue = Color.FromRgbInt(0x0000FF);
            ColorShouldEqualValues(ref blue, 0, 0, 1, nameof(blue));

            Color whiteAlpha = Color.FromRgbInt(0x000000);
            ColorShouldEqualValues(ref whiteAlpha, 0, 0, 0, nameof(whiteAlpha));
            Color blackAlpha = Color.FromRgbInt(0xFFFFFF);
            ColorShouldEqualValues(ref blackAlpha, 1, 1, 1, nameof(blackAlpha));
            Color redAlpha = Color.FromRgbInt(0x00FF0000);
            ColorShouldEqualValues(ref redAlpha, 1, 0, 0, nameof(redAlpha));
            Color greenAlpha = Color.FromRgbInt(0x0000FF00);
            ColorShouldEqualValues(ref greenAlpha, 0, 1, 0, nameof(greenAlpha));
            Color blueAlpha = Color.FromRgbInt(0x000000FF);
            ColorShouldEqualValues(ref blueAlpha, 0, 0, 1, nameof(blueAlpha));

            Color teal = Color.FromRgbInt(0x66FFCC);
            ColorShouldEqualValues(ref teal, 0.40000004f, 1, 0.8000001f, nameof(teal));
            Color tealAlpha = Color.FromRgbInt(0xFF66FFCC);
            ColorShouldEqualValues(ref tealAlpha, 0.40000004f, 1, 0.8000001f, nameof(tealAlpha));
        }

        [Test, Order(4)]
        public void ColorToUInt()
        {
            Color white = new(0, 0, 0);
            Assert.AreEqual(0xFF000000, white.ToUInt());
            Color black = new(1, 1, 1);
            Assert.AreEqual(0xFFFFFFFF, black.ToUInt());
            Color red = new(1, 0, 0);
            Assert.AreEqual(0xFFFF0000, red.ToUInt());
            Color green = new(0, 1, 0);
            Assert.AreEqual(0xFF00FF00, green.ToUInt());
            Color blue = new(0, 0, 1);
            Assert.AreEqual(0xFF0000FF, blue.ToUInt());

            Color teal = new(0.40000004f, 1, 0.8000001f);
            Assert.AreEqual(0xFF66FFCC, teal.ToUInt());
        }
    }
}