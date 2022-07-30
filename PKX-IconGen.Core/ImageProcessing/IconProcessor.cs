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
using System.IO;
using System.Threading.Tasks;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.ImageProcessing.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Convolution;

namespace PKXIconGen.Core.ImageProcessing
{
    public class IconProcessor
    {
        private enum IconMode : byte
        {
            Normal = 0,
            NormalSecondary = 1,
            Shiny = 2,
            ShinySecondary = 3
        }
        
        private RenderJob Job { get; set; }
        private bool HasSecondary => Job.Data.Render.SecondaryCamera.HasValue;

        private string FinalOutput { get; init; }

        private Game Game { get; set; }

        public IconProcessor(RenderJob job, string finalOutput)
        {
            Job = job;
            FinalOutput = finalOutput;
            Game = job.Game;
        }

        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        public async Task ProcessJobAsync()
        {
            Task<Image> mainTask = ProcessIconAsync(Job.MainPath, IconMode.Normal);
            Task<Image> shinyTask = ProcessIconAsync(Job.ShinyPath, IconMode.Shiny);
            Task<Image>? secondaryTask = HasSecondary ? ProcessIconAsync(Job.SecondaryPath, IconMode.NormalSecondary) : null;
            Task<Image>? shinySecondaryTask = HasSecondary ? ProcessIconAsync(Job.ShinySecondaryPath, IconMode.ShinySecondary) : null;

            Image[] mainImages = await Task.WhenAll(mainTask, shinyTask);
            using Image main = mainImages[0];
            using Image shiny = mainImages[1];
            main.Mutate(ctx => ctx.AddImageBottom(shiny));

            if (secondaryTask != null && shinySecondaryTask != null)
            {
                Image[] secondaryImages = await Task.WhenAll(secondaryTask, shinySecondaryTask);
                using Image secondary = secondaryImages[0];
                using Image shinySecondary = secondaryImages[1];
                secondary.Mutate(ctx => ctx.AddImageBottom(shinySecondary));

                main.Mutate(ctx => ctx.AddImageRight(secondary));
            }

            await main.SaveAsPngAsync(Path.Combine(FinalOutput, Job.Data.Output + ".png"));
        }

        private async Task<Image> ProcessIconAsync(string path, IconMode mode)
        {
            Image img = await Image.LoadAsync(path);
            byte scale = (byte)Job.Scale;
            float glowIntensity = (float)Math.Round(Math.Pow(scale * 2, 0.80));
            CoreManager.Logger.Debug("Glow Radius: {Radius}", glowIntensity);
            
            switch (mode)
            {
                case IconMode.Normal or IconMode.NormalSecondary:
                    img.Mutate(ctx =>
                    {
                        if (Job.Data.Render.Glow.Alpha != 0)
                        {
                            ctx.ApplyEdgeGlow(Job.Data.Render.Glow.ToPixel<RgbaVector>(), glowIntensity);
                        }
                        
                        if (Job.Data.Render.Background.Alpha != 0)
                        {
                            using Image background = new Image<Rgba32>(img.Width, img.Height, Job.Data.Render.Background.ToPixel<Rgba32>());
                            ctx.AddImageBehind(background);
                        }
                    });
                    break;

                case IconMode.Shiny or IconMode.ShinySecondary:
                    img.Mutate(ctx =>
                    {
                        if (Job.Data.Shiny.Render.Glow.Alpha != 0)
                        {
                            ctx.ApplyEdgeGlow(Job.Data.Shiny.Render.Glow.ToPixel<RgbaVector>(), glowIntensity);
                        }
                        
                        if (Job.Data.Shiny.Render.Background.Alpha != 0)
                        {
                            using Image background = new Image<Rgba32>(img.Width, img.Height, Job.Data.Shiny.Render.Background.ToPixel<Rgba32>());
                            ctx.AddImageBehind(background);
                        }
                    });
                    break;
                
                default:
                    throw new InvalidOperationException("Selected mode was somehow not a value of the enum.");
            }
            
            return await GameProcess(img);
        }

        private Task<Image> GameProcess(Image img)
        {
            return Task.Run(() =>
            {
                switch (Game)
                {
                    case Game.PokemonColosseum:
                        break;

                    case Game.PokemonXDGaleOfDarkness:
                        img.Mutate(ctx => ctx.CircleCrop());
                        break;

                    case Game.PokemonBattleRevolution:
                        throw new NotImplementedException("Pokemon Battle Revolution is not yet implemented.");

                    case Game.Undefined or _:
                        throw new InvalidOperationException("Selected style was 'Undefined'.");
                }
                return img;
            });
        }

        /*public void Dispose()
        {
            // We don't need ImageSharp unless we use IconProcessor again.
            Configuration.Default.MemoryAllocator.ReleaseRetainedResources();
        }*/
    }
}
