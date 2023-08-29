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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;

namespace PKXIconGen.Core.ImageProcessing.Extensions;

public static class ImageDilateExtensions
{
    public static IImageProcessingContext Dilate(this IImageProcessingContext ctx, int size)
    {
        return ctx.ApplyProcessor(new DilateProcessor(size));
    }

    private class DilateProcessor : IImageProcessor
    {
        private int Size { get; }
        
        public DilateProcessor(int size)
        {
            Size = size;
        }

        public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Configuration configuration, Image<TPixel> source,
            Rectangle sourceRectangle) where TPixel : unmanaged, IPixel<TPixel>
        {
            return new DilateProcessor<TPixel>(configuration, source, sourceRectangle, Size);
        }
    }
    
    private class DilateProcessor<TPixel> : IImageProcessor<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private Configuration Configuration { get; }
        private Image<TPixel> Source { get; }
        private Rectangle SourceRectangle { get; }
        
        private int Size { get; }
        
        public DilateProcessor(Configuration configuration, Image<TPixel> source, Rectangle sourceRectangle, int size)
        {
            Configuration = configuration;
            Source = source;
            SourceRectangle = sourceRectangle;

            Size = size;
        }
        
        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public void Execute()
        {
            if (Size == 0)
            {
                return;
            }
            
            using Image<TPixel> target = Source.Clone<TPixel>();
            target.ProcessPixelRows(Source, (sourceAccessor, targetAccessor) =>
            {
                for (int rowIndex = 0; rowIndex < sourceAccessor.Height; rowIndex++)
                {
                    Span<TPixel> rowSource = sourceAccessor.GetRowSpan(rowIndex);
                    Span<TPixel> rowTarget = targetAccessor.GetRowSpan(rowIndex);

                    for (int colIndex = 0; colIndex < sourceAccessor.Width; colIndex++)
                    {
                        Vector4 pixelData = rowSource[colIndex].ToVector4();
                        float maxX = pixelData.X;
                        float maxY = pixelData.Y;
                        float maxZ = pixelData.Z;
                        float maxW = pixelData.W;
                        
                        for (int rowSize = Size * -1; rowSize <= Size; rowSize++)
                        {
                            int compareRowIndex = rowIndex + rowSize;
                            if (compareRowIndex >= 0 && compareRowIndex < sourceAccessor.Height)
                            {
                                Span<TPixel> r = sourceAccessor.GetRowSpan(compareRowIndex);
                                Vector4 compareToFromRow = r[colIndex].ToVector4();
                                maxX = Math.Max(compareToFromRow.X, maxX);
                                maxY = Math.Max(compareToFromRow.Y, maxY);
                                maxZ = Math.Max(compareToFromRow.Z, maxZ);
                                maxW = Math.Max(compareToFromRow.W, maxW);
                                
                                for (int colSize = Size * -1; colSize <= Size; colSize++)
                                {
                                    int compareColIndex = colIndex + colSize;
                                    if (compareColIndex >= 0 && compareColIndex < sourceAccessor.Width)
                                    {
                                        Vector4 compareToFromCol = r[compareColIndex].ToVector4();
                                        maxX = Math.Max(compareToFromCol.X, maxX);
                                        maxY = Math.Max(compareToFromCol.Y, maxY);
                                        maxZ = Math.Max(compareToFromCol.Z, maxZ);
                                        maxW = Math.Max(compareToFromCol.W, maxW);
                                    }
                                }
                            }
                        }

                        pixelData.X = maxX;
                        pixelData.Y = maxY;
                        pixelData.Z = maxZ;
                        pixelData.W = maxW;
                        rowTarget[colIndex].FromVector4(pixelData);
                    }
                }
            });
        }

        public void Dispose()
        {
            
        }
    }
}