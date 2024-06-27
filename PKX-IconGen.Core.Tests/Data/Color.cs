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

namespace PKXIconGen.Core.Tests.Data;

public class ColorTests
{
    private static void ColorShouldEqualValues(ref Color color, float expectedR, float expectedG, float expectedB, float expectedA, string assertMessage)
    {
        Assert.That(color.Red, Is.EqualTo(expectedR), assertMessage + ", Red");
        Assert.That(color.Green, Is.EqualTo(expectedG), assertMessage + ", Green");
        Assert.That(color.Blue, Is.EqualTo(expectedB), assertMessage + ", Blue");
        Assert.That(color.Alpha, Is.EqualTo(expectedA), assertMessage + ", Alpha");
    }

    [Test, Order(1)]
    public void ColorClampValues()
    {
        Color minR = new(-0.001f, 0, 0, 0);
        ColorShouldEqualValues(ref minR, 0, 0, 0, 0, nameof(minR));
        Color minG = new(0, -0.001f, 0, 0);
        ColorShouldEqualValues(ref minG, 0, 0, 0, 0, nameof(minG));
        Color minB = new(0, 0, -0.001f, 0);
        ColorShouldEqualValues(ref minB, 0, 0, 0, 0, nameof(minB));
        Color minA = new(0, 0, 0, -0.001f);
        ColorShouldEqualValues(ref minA, 0, 0, 0, 0, nameof(minA));

        Color maxR = new(1.001f, 0, 0, 0);
        ColorShouldEqualValues(ref maxR, 1, 0, 0, 0, nameof(minR));
        Color maxG = new(0, 1.001f, 0, 0);
        ColorShouldEqualValues(ref maxG, 0, 1, 0, 0, nameof(minG));
        Color maxB = new(0, 0, 1.001f, 0); 
        ColorShouldEqualValues(ref maxB, 0, 0, 1, 0, nameof(minB));
        Color maxA = new(0, 0, 0, 1.001f); 
        ColorShouldEqualValues(ref maxA, 0, 0, 0, 1, nameof(maxA));

        Color teal = new(0.40000004f, 1, 0.8000001f, 1);
        ColorShouldEqualValues(ref teal, 0.40000004f, 1, 0.8000001f, 1, nameof(teal));
        Color tealAlpha = new(0.40000004f, 1, 0.8000001f, 0);
        ColorShouldEqualValues(ref tealAlpha, 0.40000004f, 1, 0.8000001f, 0, nameof(tealAlpha));
        Color tealSemiAlpha = new(0.40000004f, 1, 0.8000001f, 0.509803951f);
        ColorShouldEqualValues(ref tealSemiAlpha, 0.40000004f, 1, 0.8000001f, 0.509803951f, nameof(tealSemiAlpha));
    }

    [Test, Order(2)]
    public void ColorFromBytes()
    {
        Color transparent = Color.FromBytes(0, 0, 0, 0);
        ColorShouldEqualValues(ref transparent, 0, 0, 0, 0, nameof(transparent));
        Color white = Color.FromBytes(0, 0, 0, 255);
        ColorShouldEqualValues(ref white, 0, 0, 0, 1, nameof(white));
        Color black = Color.FromBytes(255, 255, 255, 255);
        ColorShouldEqualValues(ref black, 1, 1, 1, 1, nameof(black));
        Color red = Color.FromBytes(255, 0, 0, 255);
        ColorShouldEqualValues(ref red, 1, 0, 0, 1, nameof(red));
        Color green = Color.FromBytes(0, 255, 0, 255);
        ColorShouldEqualValues(ref green, 0, 1, 0, 1, nameof(green));
        Color blue = Color.FromBytes(0, 0, 255, 255);
        ColorShouldEqualValues(ref blue, 0, 0, 1, 1, nameof(blue));

        Color teal = Color.FromBytes(102, 255, 204, 255);
        ColorShouldEqualValues(ref teal, 0.40000004f, 1, 0.8000001f, 1, nameof(teal));
        Color tealAlpha = Color.FromBytes(102, 255, 204, 0);
        ColorShouldEqualValues(ref tealAlpha, 0.40000004f, 1, 0.8000001f, 0, nameof(tealAlpha));
        Color tealSemiAlpha = Color.FromBytes(102, 255, 204, 130);
        ColorShouldEqualValues(ref tealSemiAlpha, 0.40000004f, 1, 0.8000001f, 0.509803951f, nameof(tealSemiAlpha));
    }

    [Test, Order(3)]
    public void ColorFromRgbaInt()
    {
        Color white = Color.FromRgbaUInt(0x00000000);
        ColorShouldEqualValues(ref white, 0, 0, 0, 0, nameof(white));
        Color black = Color.FromRgbaUInt(0xFFFFFFFF);
        ColorShouldEqualValues(ref black, 1, 1, 1, 1, nameof(black));
        Color red = Color.FromRgbaUInt(0xFF000000);
        ColorShouldEqualValues(ref red, 1, 0, 0, 0, nameof(red));
        Color green = Color.FromRgbaUInt(0x00FF0000);
        ColorShouldEqualValues(ref green, 0, 1, 0, 0, nameof(green));
        Color blue = Color.FromRgbaUInt(0x0000FF00);
        ColorShouldEqualValues(ref blue, 0, 0, 1, 0,nameof(blue));
        Color alpha = Color.FromRgbaUInt(0x000000FF);
        ColorShouldEqualValues(ref alpha, 0, 0, 0, 1,nameof(alpha));

        Color teal = Color.FromRgbaUInt(0x66FFCCFF);
        ColorShouldEqualValues(ref teal, 0.40000004f, 1, 0.8000001f, 1, nameof(teal));
        Color tealAlpha = Color.FromRgbaUInt(0x66FFCC00);
        ColorShouldEqualValues(ref tealAlpha, 0.40000004f, 1, 0.8000001f, 0, nameof(tealAlpha));
        Color tealSemiAlpha = Color.FromRgbaUInt(0x66FFCC82);
        ColorShouldEqualValues(ref tealSemiAlpha, 0.40000004f, 1, 0.8000001f, 0.509803951f, nameof(tealSemiAlpha));
    }
        
    [Test, Order(4)]
    public void ColorFromArgbInt()
    {
        Color white = Color.FromArgbUInt(0xFF000000);
        ColorShouldEqualValues(ref white, 0, 0, 0, 1, nameof(white));
        Color black = Color.FromArgbUInt(0xFFFFFFFF);
        ColorShouldEqualValues(ref black, 1, 1, 1, 1, nameof(black));
        Color red = Color.FromArgbUInt(0x00FF0000);
        ColorShouldEqualValues(ref red, 1, 0, 0, 0, nameof(red));
        Color green = Color.FromArgbUInt(0x0000FF00);
        ColorShouldEqualValues(ref green, 0, 1, 0, 0, nameof(green));
        Color blue = Color.FromArgbUInt(0x000000FF);
        ColorShouldEqualValues(ref blue, 0, 0, 1, 0,nameof(blue));
        Color alpha = Color.FromArgbUInt(0xFF000000);
        ColorShouldEqualValues(ref alpha, 0, 0, 0, 1,nameof(alpha));

        Color teal = Color.FromArgbUInt(0xFF66FFCC);
        ColorShouldEqualValues(ref teal, 0.40000004f, 1, 0.8000001f, 1, nameof(teal));
        Color tealAlpha = Color.FromArgbUInt(0x0066FFCC);
        ColorShouldEqualValues(ref tealAlpha, 0.40000004f, 1, 0.8000001f, 0, nameof(tealAlpha));
        Color tealSemiAlpha = Color.FromArgbUInt(0x8266FFCC);
        ColorShouldEqualValues(ref tealSemiAlpha, 0.40000004f, 1, 0.8000001f, 0.509803951f, nameof(tealSemiAlpha));
    }

    [Test, Order(5)]
    public void ColorToRgbaUInt()
    {
        Color white = new(0, 0, 0, 1);
        Assert.That(white.ToRgbaUInt(), Is.EqualTo(0x000000FF));
        Color black = new(1, 1, 1, 1);
        Assert.That(black.ToRgbaUInt(), Is.EqualTo(0xFFFFFFFF));
        Color red = new(1, 0, 0, 1);
        Assert.That(red.ToRgbaUInt(), Is.EqualTo(0xFF0000FF));
        Color green = new(0, 1, 0, 1);
        Assert.That(green.ToRgbaUInt(), Is.EqualTo(0x00FF00FF));
        Color blue = new(0, 0, 1, 1);
        Assert.That(blue.ToRgbaUInt(), Is.EqualTo(0x0000FFFF));

        Color teal = new(0.40000004f, 1, 0.8000001f, 1);
        Assert.That(teal.ToRgbaUInt(), Is.EqualTo(0x66FFCCFF));
        Color tealAlpha = new(0.40000004f, 1, 0.8000001f, 0);
        Assert.That(tealAlpha.ToRgbaUInt(), Is.EqualTo(0x66FFCC00));
        Color tealSemiAlpha = new(0.40000004f, 1, 0.8000001f, 0.509803951f);
        Assert.That(tealSemiAlpha.ToRgbaUInt(), Is.EqualTo(0x66FFCC82));
    }
        
    [Test, Order(6)]
    public void ColorToArgbUInt()
    {
        Color white = new(0, 0, 0, 1);
        Assert.That(white.ToArgbUInt(), Is.EqualTo(0xFF000000));
        Color black = new(1, 1, 1, 1);
        Assert.That(black.ToArgbUInt(), Is.EqualTo(0xFFFFFFFF));
        Color red = new(1, 0, 0, 1);
        Assert.That(red.ToArgbUInt(), Is.EqualTo(0xFFFF0000));
        Color green = new(0, 1, 0, 1);
        Assert.That(green.ToArgbUInt(), Is.EqualTo(0xFF00FF00));
        Color blue = new(0, 0, 1, 1);
        Assert.That(blue.ToArgbUInt(), Is.EqualTo(0xFF0000FF));

        Color teal = new(0.40000004f, 1, 0.8000001f, 1);
        Assert.That(teal.ToArgbUInt(), Is.EqualTo(0xFF66FFCC));
        Color tealAlpha = new(0.40000004f, 1, 0.8000001f, 0);
        Assert.That(tealAlpha.ToArgbUInt(), Is.EqualTo(0x0066FFCC));
        Color tealSemiAlpha = new(0.40000004f, 1, 0.8000001f, 0.509803951f);
        Assert.That(tealSemiAlpha.ToArgbUInt(), Is.EqualTo(0x8266FFCC));
    }
}