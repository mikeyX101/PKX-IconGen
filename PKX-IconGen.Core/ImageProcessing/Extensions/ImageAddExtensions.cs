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
using SixLabors.ImageSharp.Processing;

namespace PKXIconGen.Core.ImageProcessing.Extensions;

public static class ImageAddExtensions
{
    public static IImageProcessingContext AddImageBottom(this IImageProcessingContext ctx, Image img)
    {
        Size size = ctx.GetCurrentSize();

        return ctx
            .Resize(new ResizeOptions
            {
                Size = new Size(size.Width, size.Height + img.Height),
                Mode = ResizeMode.BoxPad,
                Position = AnchorPositionMode.TopLeft
            })
            .DrawImage(img, new Point(0, size.Height), 1f);
    }

    public static IImageProcessingContext AddImageRight(this IImageProcessingContext ctx, Image img)
    {
        Size size = ctx.GetCurrentSize();

        return ctx
            .Resize(new ResizeOptions
            {
                Size = new Size(size.Width + img.Width, size.Height),
                Mode = ResizeMode.BoxPad,
                Position = AnchorPositionMode.TopLeft
            })
            .DrawImage(img, new Point(size.Width, 0), 1f);
    }
        
    public static IImageProcessingContext AddImageBehind(this IImageProcessingContext ctx, Image img)
    {
        return ctx.DrawImage(img, new Point(0, 0), PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.DestOver, 1f);
    }
}