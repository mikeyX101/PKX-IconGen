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
using System.Diagnostics.CodeAnalysis;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;

namespace PKXIconGen.Core.ImageProcessing.Extensions;

public static class ImageEdgeGlowExtensions
{
    public static IImageProcessingContext ApplyEdgeGlow(this IImageProcessingContext ctx, RgbaVector glowColor, float glowIntensity)
    {
        return ctx.ApplyProcessor(new EdgeGlowProcessor(glowColor, glowIntensity));
    }
    
    private static IImageProcessingContext ApplyEdgeGlowBlur(this IImageProcessingContext ctx, float glowIntensity, Rectangle sourceRectangle)
    {
        return ctx.GaussianBlur(glowIntensity, sourceRectangle);//.BokehBlur(glowIntensity, 6, 1f, SourceRectangle));
    }

    private class EdgeGlowProcessor : IImageProcessor
    {
        private RgbaVector GlowColor { get; }
        private float GlowIntensity { get; }
        
        public EdgeGlowProcessor(RgbaVector glowColor, float glowIntensity)
        {
            GlowColor = glowColor;
            GlowIntensity = glowIntensity;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source,
            Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new EdgeGlowProcessor<TPixel>(configuration, source, sourceRectangle, GlowColor, GlowIntensity);
        }
    }
    
    private class EdgeGlowProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private Configuration Configuration { get; }
        private Image<TPixel> Source { get; }
        private Rectangle SourceRectangle { get; }
        
        private RgbaVector GlowColor { get; }
        private float GlowIntensity { get; }
        
        public EdgeGlowProcessor(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle, RgbaVector glowColor, float glowIntensity)
        {
            Configuration = configuration;
            Source = source;
            SourceRectangle = sourceRectangle;

            GlowColor = glowColor;
            GlowIntensity = glowIntensity;
        }
        
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void Execute()
        {
            using Image<TPixel> glowEffect = Source.Clone<TPixel>();
            
            glowEffect.Mutate(ctx => 
                ctx.ProcessPixelRowsAsVector4(row =>
                    {
                        for (int x = 0; x < row.Length; x++)
                        {
                            row[x].X = GlowColor.R;
                            row[x].Y = GlowColor.G;
                            row[x].Z = GlowColor.B;
                            row[x].W = row[x].W == 0 ? 0 : (row[x].W + GlowColor.A) / 2; // Average alpha with glow color
                        }
                    }, SourceRectangle, PixelConversionModifiers.Premultiply)
                    #if !DEBUG
                    .ApplyEdgeGlowBlur(GlowIntensity, SourceRectangle)
                    #endif
            );
            
            #if DEBUG
            glowEffect.SaveAsPng(System.IO.Path.Combine(Paths.TempFolder, "glowEffectBeforeBlur.png"));
            glowEffect.Mutate(ctx => ctx.ApplyEdgeGlowBlur(GlowIntensity, SourceRectangle));
            glowEffect.SaveAsPng(System.IO.Path.Combine(Paths.TempFolder, "glowEffectAfterBlur.png"));
            #endif
            
            Source.Mutate(ctx => ctx.AddImageBehind(glowEffect));
        }

        public void Dispose()
        {
            
        }
    }
}