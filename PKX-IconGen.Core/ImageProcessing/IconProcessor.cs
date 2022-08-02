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
using System.Threading;
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
        public async Task ProcessJobAsync(CancellationToken? token = null, Action<ReadOnlyMemory<char>>? stepOutput = null)
        {
            Task<Image> mainTask = ProcessIconAsync(Job.MainPath, IconMode.Normal, token, stepOutput);
            Task<Image> shinyTask = ProcessIconAsync(Job.ShinyPath, IconMode.Shiny, token, stepOutput);
            Task<Image>? secondaryTask = HasSecondary ? ProcessIconAsync(Job.SecondaryPath, IconMode.NormalSecondary, token, stepOutput) : null;
            Task<Image>? shinySecondaryTask = HasSecondary ? ProcessIconAsync(Job.ShinySecondaryPath, IconMode.ShinySecondary, token, stepOutput) : null;

            Image[] mainImages = await Task.WhenAll(mainTask, shinyTask);
            using Image main = mainImages[0];
            using Image shiny = mainImages[1];
            
            token?.ThrowIfCancellationRequested();
            stepOutput?.Invoke($"Combining images for {Job.Data.Output}...".AsMemory());
            CoreManager.Logger.Information("Combining images for {Name}...", Job.Data.Output);
            main.Mutate(ctx => ctx.AddImageBottom(shiny));

            if (secondaryTask != null && shinySecondaryTask != null)
            {
                Image[] secondaryImages = await Task.WhenAll(secondaryTask, shinySecondaryTask);
                using Image secondary = secondaryImages[0];
                using Image shinySecondary = secondaryImages[1];
                secondary.Mutate(ctx => ctx.AddImageBottom(shinySecondary));

                main.Mutate(ctx => ctx.AddImageRight(secondary));
            }
            CoreManager.Logger.Information("Combining images for {Name}...Done!", Job.Data.Output);

            await main.SaveAsPngAsync(Path.Combine(FinalOutput, Job.Data.Output + ".png"));
            stepOutput?.Invoke($"Finished rendering {Job.Data.Output}!".AsMemory());
            CoreManager.Logger.Information("Finished rendering {Name}!", Job.Data.Output);
        }

        private async Task<Image> ProcessIconAsync(string path, IconMode mode, CancellationToken? token = null, Action<ReadOnlyMemory<char>>? stepOutput = null)
        {
            Image img = await Image.LoadAsync(path);
            byte scale = (byte)Job.Scale;
            float glowIntensity = (float)Math.Round(Math.Pow(scale * 2, 0.80));
            token?.ThrowIfCancellationRequested();
            
            switch (mode)
            {
                case IconMode.Normal or IconMode.NormalSecondary:
                    img.Mutate(ctx =>
                    {
                        if (Job.Data.Render.Glow.Alpha != 0)
                        {
                            stepOutput?.Invoke($"Applying glow to {mode} image for {Job.Data.Output}...".AsMemory());
                            CoreManager.Logger.Information("Applying glow to {Mode} image for {Name}...", mode, Job.Data.Output);
                            ctx.ApplyEdgeGlow(Job.Data.Render.Glow.ToPixel<RgbaVector>(), glowIntensity);
                            CoreManager.Logger.Information("Applying glow to {Mode} image for {Name}...Done!", mode, Job.Data.Output);
                        }
                        token?.ThrowIfCancellationRequested();
                        if (Job.Data.Render.Background.Alpha != 0)
                        {
                            stepOutput?.Invoke($"Applying background color to {mode} image for {Job.Data.Output}...".AsMemory());
                            CoreManager.Logger.Information("Applying background color to {Mode} image for {Name}...", mode, Job.Data.Output);
                            using Image background = new Image<Rgba32>(img.Width, img.Height, Job.Data.Render.Background.ToPixel<Rgba32>());
                            ctx.AddImageBehind(background);
                            CoreManager.Logger.Information("Applying background color to {Mode} image for {Name}...Done!", mode, Job.Data.Output);
                        }
                    });
                    break;

                case IconMode.Shiny or IconMode.ShinySecondary:
                    img.Mutate(ctx =>
                    {
                        if (Job.Data.Shiny.Render.Glow.Alpha != 0)
                        {
                            stepOutput?.Invoke($"Applying glow to {mode} image for {Job.Data.Output}...".AsMemory());
                            CoreManager.Logger.Information("Applying glow to {Mode} image for {Name}...", mode, Job.Data.Output);
                            ctx.ApplyEdgeGlow(Job.Data.Shiny.Render.Glow.ToPixel<RgbaVector>(), glowIntensity);
                            CoreManager.Logger.Information("Applying glow to {Mode} image for {Name}...Done!", mode, Job.Data.Output);
                        }
                        token?.ThrowIfCancellationRequested();
                        if (Job.Data.Shiny.Render.Background.Alpha != 0)
                        {
                            stepOutput?.Invoke($"Applying background color to {mode} image for {Job.Data.Output}...".AsMemory());
                            CoreManager.Logger.Information("Applying background color to {Mode} image for {Name}...", mode, Job.Data.Output);
                            using Image background = new Image<Rgba32>(img.Width, img.Height, Job.Data.Shiny.Render.Background.ToPixel<Rgba32>());
                            ctx.AddImageBehind(background);
                            CoreManager.Logger.Information("Applying background color to {Mode} image for {Name}...Done!", mode, Job.Data.Output);
                        }
                    });
                    break;
                
                default:
                    throw new InvalidOperationException("Selected mode was somehow not a value of the enum.");
            }
            token?.ThrowIfCancellationRequested();
            
            return await GameProcess(img, stepOutput);
        }

        private Task<Image> GameProcess(Image img, Action<ReadOnlyMemory<char>>? stepOutput = null)
        {
            return Task.Run(() =>
            {
                stepOutput?.Invoke($"Applying style for game {Game} for {Job.Data.Output}...".AsMemory());
                CoreManager.Logger.Information("Applying style for game {Game} for {Name}...", Game, Job.Data.Output);
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
                CoreManager.Logger.Information("Applying style for game {Game} for {Name}...Done!", Game, Job.Data.Output);
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
