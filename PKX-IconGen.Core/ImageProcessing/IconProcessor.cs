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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.ImageProcessing.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PKXIconGen.Core.ImageProcessing
{
    [Flags]
    public enum Icon : short
    {
        FaceNormal = 1,
        FaceNormalSecondary = 2,
        FaceShiny = 4,
        FaceShinySecondary = 8,
            
        BoxFirst = 16,
        BoxFirstShiny = 32,
        BoxSecond = 64,
        BoxSecondShiny = 128,
        BoxThird = 256,
        BoxThirdShiny = 512,
        
        AnyFace = FaceNormal | FaceNormalSecondary | FaceShiny | FaceShinySecondary,
        AnyBox = BoxFirst | BoxFirstShiny | BoxSecond | BoxSecondShiny | BoxThird | BoxThirdShiny
    }

    public static class IconExtensions
    {
        public static RenderData GetIconRenderData(this Icon icon, PokemonRenderData prd) => icon switch
        {
            Icon.FaceNormal or Icon.FaceNormalSecondary => prd.FaceRender,
            Icon.FaceShiny or Icon.FaceShinySecondary => prd.Shiny.FaceRender,
            Icon.BoxFirst => prd.BoxRender.First,
            Icon.BoxFirstShiny => prd.Shiny.BoxRender.First,
            Icon.BoxSecond => prd.BoxRender.Second,
            Icon.BoxSecondShiny => prd.Shiny.BoxRender.Second,
            Icon.BoxThird => prd.BoxRender.Third,
            Icon.BoxThirdShiny => prd.Shiny.BoxRender.Third,
            _ => throw new ArgumentOutOfRangeException(nameof(icon), "Unknown Icon")
        };
    }
    
    public class IconProcessor
    {
        private RenderJob Job { get; set; }
        private bool HasSecondary => Job.Data.FaceRender.SecondaryCamera.HasValue;

        private string FinalOutput { get; init; }
        
        private bool SaturationBoost { get; init; }

        private Game Game => Job.Game;
        private RenderTarget Target => Job.Target;
        
        
        public IconProcessor(RenderJob job, string finalOutput, bool saturationBoost)
        {
            Job = job;
            FinalOutput = finalOutput;
            SaturationBoost = saturationBoost;
        }
        
        public async Task ProcessJobAsync(CancellationToken? token = null, Action<ReadOnlyMemory<char>>? stepOutput = null)
        {
            List<Task<Image>> faceTasks = new();
            List<Task<Image>> boxTasks = new();
            if (Target.HasFlag(RenderTarget.Face))
            {
                faceTasks.Add(ProcessIconAsync(Job.FaceMainPath, Icon.FaceNormal, token, stepOutput));
                faceTasks.Add(ProcessIconAsync(Job.FaceShinyPath, Icon.FaceShiny, token, stepOutput));

                if (HasSecondary)
                {
                    faceTasks.Add(ProcessIconAsync(Job.FaceSecondaryPath, Icon.FaceNormalSecondary, token, stepOutput));
                    faceTasks.Add(ProcessIconAsync(Job.FaceShinySecondaryPath, Icon.FaceShinySecondary, token, stepOutput));
                }
            }
            
            if (Target.HasFlag(RenderTarget.Box))
            {
                boxTasks.Add(ProcessIconAsync(Job.BoxFirstMainPath, Icon.BoxFirst, token, stepOutput));
                boxTasks.Add(ProcessIconAsync(Job.BoxFirstShinyPath, Icon.BoxFirstShiny, token, stepOutput));
                if (Game == Game.PokemonXDGaleOfDarkness)
                {
                    boxTasks.Add(ProcessIconAsync(Job.BoxSecondMainPath, Icon.BoxSecond, token, stepOutput));
                    boxTasks.Add(ProcessIconAsync(Job.BoxSecondShinyPath, Icon.BoxSecondShiny, token, stepOutput));
                    boxTasks.Add(ProcessIconAsync(Job.BoxThirdMainPath, Icon.BoxThird, token, stepOutput));
                    boxTasks.Add(ProcessIconAsync(Job.BoxThirdShinyPath, Icon.BoxThirdShiny, token, stepOutput));
                }
            }

            Task faceProcessing = Task.CompletedTask;
            if (faceTasks.Count > 0)
            {
                faceProcessing = ProcessFaceAsync(faceTasks);
            }
            
            Task boxProcessing = Task.CompletedTask;
            if (boxTasks.Count > 0)
            {
                boxProcessing = ProcessBoxAsync(boxTasks);
            }

            await Task.WhenAll(faceProcessing, boxProcessing);
        }

        private async Task ProcessFaceAsync(IReadOnlyList<Task<Image>> faceTasks, CancellationToken? token = null, Action<ReadOnlyMemory<char>>? stepOutput = null)
        {
            Image[] mainImages = await Task.WhenAll(faceTasks[0], faceTasks[1]);
            Image main = mainImages[0];
            Image shiny = mainImages[1];
            
            token?.ThrowIfCancellationRequested();
            stepOutput?.Invoke($"Combining face images for {Job.Data.Name}...".AsMemory());
            CoreManager.Logger.Information("Combining face images for {Output} ({Name})...", Job.Data.FaceOutput, Job.Data.Name);
            main.Mutate(ctx => ctx.AddImageBottom(shiny));

            if (HasSecondary)
            {
                token?.ThrowIfCancellationRequested();
                
                Image[] secondaryImages = await Task.WhenAll(faceTasks[3], faceTasks[4]);
                Image secondary = secondaryImages[0];
                Image shinySecondary = secondaryImages[1];
                secondary.Mutate(ctx => ctx.AddImageBottom(shinySecondary));

                main.Mutate(ctx => ctx.AddImageRight(secondary));
            }
            CoreManager.Logger.Information("Combining face images for {Output} ({Name})...Done!", Job.Data.FaceOutput, Job.Data.Name);

            await main.SaveAsPngAsync(Path.Combine(FinalOutput, Job.Data.FaceOutput + ".png"));
            stepOutput?.Invoke($"Finished rendering face {Job.Data.Name}!".AsMemory());
            CoreManager.Logger.Information("Finished rendering face {Output} ({Name})!", Job.Data.FaceOutput, Job.Data.Name);
        }
        
        private async Task ProcessBoxAsync(IReadOnlyList<Task<Image>> boxImages, CancellationToken? token = null, Action<ReadOnlyMemory<char>>? stepOutput = null)
        {
            Image[] firstBoxImages = await Task.WhenAll(boxImages[0], boxImages[1]);
            Image bodyFirst = firstBoxImages[0];
            Image bodyFirstShiny = firstBoxImages[1];
            
            if (Game == Game.PokemonXDGaleOfDarkness)
            {
                token?.ThrowIfCancellationRequested();
                
                Image[] otherBoxImages = await Task.WhenAll(boxImages[2], boxImages[3], boxImages[4], boxImages[5]);
                Image second = otherBoxImages[0];
                Image secondShiny = otherBoxImages[1];
                Image third = otherBoxImages[2];
                Image thirdShiny = otherBoxImages[3];
                
                stepOutput?.Invoke($"Combining box images for {Job.Data.Name}...".AsMemory());
                CoreManager.Logger.Information("Combining box images for {Output} ({Name})...", Job.Data.DanceOutput, Job.Data.Name);
                Image danceFirst = bodyFirst.Clone(ctx => ctx.AddImageBottom(second).AddImageBottom(third));
                CoreManager.Logger.Information("Combining box images for {Output} ({Name})...Done!", Job.Data.FaceOutput, Job.Data.Name);
                CoreManager.Logger.Information("Combining box images for {Output} ({Name})...", Job.Data.DanceShinyOutput, Job.Data.Name);
                Image danceFirstShiny = bodyFirstShiny.Clone(ctx => ctx.AddImageBottom(secondShiny).AddImageBottom(thirdShiny));
                CoreManager.Logger.Information("Combining box images for {Output} ({Name})...Done!", Job.Data.DanceShinyOutput, Job.Data.Name);
                
                await danceFirst.SaveAsPngAsync(Path.Combine(FinalOutput, Job.Data.DanceOutput + ".png"));
                await danceFirstShiny.SaveAsPngAsync(Path.Combine(FinalOutput, Job.Data.DanceShinyOutput + ".png"));
            }
            
            await bodyFirst.SaveAsPngAsync(Path.Combine(FinalOutput, Job.Data.BodyOutput + ".png"));
            await bodyFirstShiny.SaveAsPngAsync(Path.Combine(FinalOutput, Job.Data.BodyShinyOutput + ".png"));
            stepOutput?.Invoke($"Finished rendering box {Job.Data.Name}!".AsMemory());
            CoreManager.Logger.Information("Finished rendering box {Output} ({Name})!", Job.Data.FaceOutput, Job.Data.Name);
        }

        private async Task<Image> ProcessIconAsync(string path, Icon mode, CancellationToken? token = null, Action<ReadOnlyMemory<char>>? stepOutput = null)
        {
            Image img = await Image.LoadAsync(path);
            if (SaturationBoost)
            {
                img.Mutate(ctx => ctx.Saturate(1.1f));
            }
            
            byte scale = (byte)Job.Scale;
            float glowIntensity = (float)Math.Round(Math.Pow(scale * 2, Icon.AnyBox.HasFlag(mode) ? 0.4 : 0.80));
            int expandByPixels = Icon.AnyBox.HasFlag(mode) ? scale : 0;
            token?.ThrowIfCancellationRequested();

            RenderData renderData = mode.GetIconRenderData(Job.Data);
            img.Mutate(ctx =>
            {
                if (renderData.Glow.Alpha != 0)
                {
                    stepOutput?.Invoke($"Applying glow to {mode} image for {Job.Data.Name}...".AsMemory());
                    CoreManager.Logger.Information("Applying glow to {Mode} image for {Output} ({Name})...", mode, Job.Data.Output, Job.Data.Name);
                    ctx.ApplyEdgeGlow(renderData.Glow.ToPixel<RgbaVector>(), glowIntensity, expandByPixels);
                    CoreManager.Logger.Information("Applying glow to {Mode} image for {Output} ({Name})...Done!", mode, Job.Data.Output, Job.Data.Name);
                }
                token?.ThrowIfCancellationRequested();
                if (renderData.Background.Alpha != 0)
                {
                    stepOutput?.Invoke($"Applying background color to {mode} image for {Job.Data.Name}...".AsMemory());
                    CoreManager.Logger.Information("Applying background color to {Mode} image for {Output} ({Name})...", mode, Job.Data.Output, Job.Data.Name);
                    using Image background = new Image<Rgba32>(img.Width, img.Height, renderData.Background.ToPixel<Rgba32>());
                    ctx.AddImageBehind(background);
                    CoreManager.Logger.Information("Applying background color to {Mode} image for {Output} ({Name})...Done!", mode, Job.Data.Output, Job.Data.Name);
                }
            });
            
            stepOutput?.Invoke($"Applying style for game {Game} for {Job.Data.Name}...".AsMemory());
            CoreManager.Logger.Information("Applying style for game {Game} for {Output} ({Name})...", Game, Job.Data.Output, Job.Data.Name);
            switch (Game)
            {
                case Game.PokemonColosseum:
                    if (Icon.AnyBox.HasFlag(mode))
                    {
                        img.Mutate(ctx => ctx.Contrast(0.9f));
                    }
                    break;

                case Game.PokemonXDGaleOfDarkness:
                    if (Icon.AnyFace.HasFlag(mode))
                    {
                        img.Mutate(ctx => ctx.PokemonXDCrop());
                    }
                    break;

                case Game.PokemonBattleRevolution:
                    throw new NotImplementedException("Pokemon Battle Revolution is not yet implemented.");

                case Game.Undefined or _:
                    throw new InvalidOperationException("Selected style was 'Undefined'.");
            }
            CoreManager.Logger.Information("Applying style for game {Game} for {Output} ({Name})...Done!", Game, Job.Data.Output, Job.Data.Name);
            return img;
        }

        /*public void Dispose()
        {
            // We don't need ImageSharp unless we use IconProcessor again.
            Configuration.Default.MemoryAllocator.ReleaseRetainedResources();
        }*/
    }
}
