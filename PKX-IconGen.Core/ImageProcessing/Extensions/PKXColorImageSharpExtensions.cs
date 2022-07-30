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

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using PKXColor = PKXIconGen.Core.Data.Blender.Color;

namespace PKXIconGen.Core.ImageProcessing.Extensions;

public static class PKXColorImageSharpExtensions
{
    public static TPixel ToPixel<TPixel>(this PKXColor color) where TPixel : unmanaged, IPixel<TPixel> => 
        Color.FromRgba(color.RValue, color.GValue, color.BValue, color.AValue).ToPixel<TPixel>();
    
}