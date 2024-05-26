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
using System.Threading;
using System.Threading.Tasks;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.ImageProcessing.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

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
    
    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    public class IconProcessor
    {
        private RenderJob Job { get; set; }
        private bool HasSecondary => Job.Data.FaceRender.SecondaryCamera.HasValue;

        private string OutputPath { get; init; }
        
        private bool SaturationBoost { get; init; }
        
        private bool SaveDanceGIF { get; init; }
        
        private Game NameForGame { get; init; }
        private TextureTargetChoice NameTarget { get; init; }

        private Game Game => Job.Game;
        private RenderTarget Target => Job.Target;
        
        
        public IconProcessor(RenderJob job, string outputPath, bool saturationBoost, bool saveDanceGif, Game nameForGame, TextureTargetChoice nameTarget)
        {
            Job = job;
            OutputPath = outputPath;
            SaturationBoost = saturationBoost;
            SaveDanceGIF = saveDanceGif;
            NameForGame = nameForGame;
            NameTarget = nameTarget;
        }
        
        public async Task ProcessJobAsync(CancellationToken? token = null, Func<ReadOnlyMemory<char>, Task>? stepOutputAsync = null)
        {
            stepOutputAsync?.Invoke($"Processing icons for {Job.Data.Name}...".AsMemory());
            
            List<Task<Image>> faceTasks = new();
            List<Task<Image>> boxTasks = new();
            if (Target.HasFlag(RenderTarget.Face))
            {
                faceTasks.Add(ProcessIconAsync(Job.FaceMainPath, Icon.FaceNormal, token));
                faceTasks.Add(ProcessIconAsync(Job.FaceShinyPath, Icon.FaceShiny, token));

                if (HasSecondary)
                {
                    faceTasks.Add(ProcessIconAsync(Job.FaceSecondaryPath, Icon.FaceNormalSecondary, token));
                    faceTasks.Add(ProcessIconAsync(Job.FaceShinySecondaryPath, Icon.FaceShinySecondary, token));
                }
            }
            
            if (Target.HasFlag(RenderTarget.Box))
            {
                boxTasks.Add(ProcessIconAsync(Job.BoxFirstMainPath, Icon.BoxFirst, token));
                boxTasks.Add(ProcessIconAsync(Job.BoxFirstShinyPath, Icon.BoxFirstShiny, token));
                if (Game == Game.PokemonXDGaleOfDarkness)
                {
                    boxTasks.Add(ProcessIconAsync(Job.BoxSecondMainPath, Icon.BoxSecond, token));
                    boxTasks.Add(ProcessIconAsync(Job.BoxSecondShinyPath, Icon.BoxSecondShiny, token));
                    boxTasks.Add(ProcessIconAsync(Job.BoxThirdMainPath, Icon.BoxThird, token));
                    boxTasks.Add(ProcessIconAsync(Job.BoxThirdShinyPath, Icon.BoxThirdShiny, token));
                }
            }

            Task faceProcessing = Task.CompletedTask;
            if (faceTasks.Count > 0)
            {
                faceProcessing = ProcessFaceAsync(faceTasks, token, stepOutputAsync);
            }
            
            Task boxProcessing = Task.CompletedTask;
            if (boxTasks.Count > 0)
            {
                boxProcessing = ProcessBoxAsync(boxTasks, token, stepOutputAsync);
            }

            await Task.WhenAll(faceProcessing, boxProcessing);
        }

        private async Task ProcessFaceAsync(IReadOnlyList<Task<Image>> faceTasks, CancellationToken? token = null, Func<ReadOnlyMemory<char>, Task>? stepOutputAsync = null)
        {
            Image[] mainImages = await Task.WhenAll(faceTasks[0], faceTasks[1]);
            using Image main = mainImages[0];
            using Image shiny = mainImages[1];
            
            token?.ThrowIfCancellationRequested();
            stepOutputAsync?.Invoke($"Combining face images for {Job.Data.Name}...".AsMemory());
            CoreManager.Logger.Information("Combining face images for {Output} ({Name})...", Job.Data.FaceOutput, Job.Data.Name);
            main.Mutate(ctx => ctx.AddImageBottom(shiny));

            if (HasSecondary)
            {
                token?.ThrowIfCancellationRequested();
                
                Image[] secondaryImages = await Task.WhenAll(faceTasks[2], faceTasks[3]);
                using Image secondary = secondaryImages[0];
                using Image shinySecondary = secondaryImages[1];
                secondary.Mutate(ctx => ctx.AddImageBottom(shinySecondary));

                main.Mutate(ctx => ctx.AddImageRight(secondary));
            }
            
            string outputName = Job.Data.GetTextureNames(NameForGame, NameTarget, OutputChoice.Face) ?? Job.Data.FaceOutput;
            await main.SaveAsPngAsync(Path.Combine(OutputPath, outputName + ".png"));
            stepOutputAsync?.Invoke($"Finished rendering face {Job.Data.Name}!".AsMemory());
            CoreManager.Logger.Information("Finished rendering face {Output} ({Name})!", Job.Data.FaceOutput, Job.Data.Name);
        }
        
        private async Task ProcessBoxAsync(IReadOnlyList<Task<Image>> boxImages, CancellationToken? token = null, Func<ReadOnlyMemory<char>, Task>? stepOutputAsync = null)
        {
            Image[] firstBoxImages = await Task.WhenAll(boxImages[0], boxImages[1]);
            using Image first = firstBoxImages[0];
            using Image firstShiny = firstBoxImages[1];
            
            if (Game == Game.PokemonXDGaleOfDarkness)
            {
                token?.ThrowIfCancellationRequested();
                
                Image[] otherBoxImages = await Task.WhenAll(boxImages[2], boxImages[3], boxImages[4], boxImages[5]);
                using Image second = otherBoxImages[0];
                using Image secondShiny = otherBoxImages[1];
                using Image third = otherBoxImages[2];
                using Image thirdShiny = otherBoxImages[3];
                
                stepOutputAsync?.Invoke($"Combining box images for {Job.Data.Name}...".AsMemory());
                CoreManager.Logger.Information("Combining box images for {Output} ({Name})...", Job.Data.DanceOutput, Job.Data.Name);
                using Image danceFirst = first.Clone(ctx => ctx.AddImageBottom(second).AddImageBottom(third));
                CoreManager.Logger.Information("Combining box images for {Output} ({Name})...", Job.Data.DanceShinyOutput, Job.Data.Name);
                using Image danceFirstShiny = firstShiny.Clone(ctx => ctx.AddImageBottom(secondShiny).AddImageBottom(thirdShiny));
                
                string outputName = (Job.Data.GetTextureNames(NameForGame, NameTarget, OutputChoice.Box) ?? Job.Data.DanceOutput) + ".png";
                string outputNameShiny = (Job.Data.GetTextureNames(NameForGame, NameTarget, OutputChoice.BoxShiny) ?? Job.Data.DanceShinyOutput) + ".png";
                
                await danceFirst.SaveAsPngAsync(Path.Combine(OutputPath, outputName));
                await danceFirstShiny.SaveAsPngAsync(Path.Combine(OutputPath, outputNameShiny));

                if (SaveDanceGIF)
                {
                    stepOutputAsync?.Invoke($"Saving dance GIF for {Job.Data.Name}...".AsMemory());
                    CoreManager.Logger.Information("Saving dance GIFs for {Output} ({Name})...", Job.Data.DanceOutput, Job.Data.Name);
                    await SaveDanceGifAsync(Job.Data.DanceOutput + ".gif", first, second, third);
                    await SaveDanceGifAsync(Job.Data.DanceShinyOutput + ".gif", firstShiny, secondShiny, thirdShiny);
                }
            }
            else
            {
                token?.ThrowIfCancellationRequested();
                
                string outputName = (Job.Data.GetTextureNames(NameForGame, NameTarget, OutputChoice.Box) ?? Job.Data.BodyOutput) + ".png";
                string outputNameShiny = (Job.Data.GetTextureNames(NameForGame, NameTarget, OutputChoice.BoxShiny) ?? Job.Data.BodyShinyOutput) + ".png";
                
                await first.SaveAsPngAsync(Path.Combine(OutputPath, outputName));
                await firstShiny.SaveAsPngAsync(Path.Combine(OutputPath, outputNameShiny));
            }
            
            
            stepOutputAsync?.Invoke($"Finished rendering box for {Job.Data.Name}!".AsMemory());
            CoreManager.Logger.Information("Finished rendering box for {Output} ({Name})!", Job.Data.FaceOutput, Job.Data.Name);
        }

        private async Task SaveDanceGifAsync(string gifName, Image first, Image second, Image third, CancellationToken? token = null)
        {
            token?.ThrowIfCancellationRequested();
            
            const int repeatCount = 0;
            const int frameDelay = 17;
            
            using Image background = new Image<Rgba32>(first.Width, first.Height, Color.LightGray);
            using Image danceGif = first.Clone(ctx => ctx.AddImageBehind(background));
            using Image secondBg = second.Clone(ctx => ctx.AddImageBehind(background));
            using Image thirdBg = third.Clone(ctx => ctx.AddImageBehind(background));
            GifMetadata gifMetaData = danceGif.Metadata.GetGifMetadata();
            gifMetaData.RepeatCount = repeatCount;
            gifMetaData.ColorTableMode = GifColorTableMode.Local;

            danceGif.Frames.AddFrame(secondBg.Frames.RootFrame);
            danceGif.Frames.AddFrame(thirdBg.Frames.RootFrame);
            danceGif.Frames.AddFrame(secondBg.Frames.RootFrame);

            foreach (ImageFrame frame in danceGif.Frames)
            {
                GifFrameMetadata metadata = frame.Metadata.GetGifMetadata();
                metadata.FrameDelay = frameDelay;
                metadata.ColorTableMode = GifColorTableMode.Local;
            }
            
            await danceGif.SaveAsGifAsync(Path.Combine(OutputPath, gifName), new GifEncoder { Quantizer = KnownQuantizers.Wu, ColorTableMode = GifColorTableMode.Local, PixelSamplingStrategy = new ExtensivePixelSamplingStrategy() }, token ?? CancellationToken.None);
        }
        
        private async Task<Image> ProcessIconAsync(string path, Icon mode, CancellationToken? token = null)
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
                    CoreManager.Logger.Information("Applying glow to {Mode} image for {Output} ({Name})...", mode, Job.Data.Output, Job.Data.Name);
                    ctx.ApplyEdgeGlow(renderData.Glow.ToPixel<RgbaVector>(), glowIntensity, expandByPixels);
                }
                token?.ThrowIfCancellationRequested();
                if (renderData.Background.Alpha != 0)
                {
                    CoreManager.Logger.Information("Applying background color to {Mode} image for {Output} ({Name})...", mode, Job.Data.Output, Job.Data.Name);
                    using Image background = new Image<Rgba32>(img.Width, img.Height, renderData.Background.ToPixel<Rgba32>());
                    ctx.AddImageBehind(background);
                }
            });
            
            CoreManager.Logger.Information("Applying style to {Mode} for game {Game} for {Output} ({Name})...", mode, Game, Job.Data.Output, Job.Data.Name);
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
            return img;
        }

        /*public void Dispose()
        {
            // We don't need ImageSharp unless we use IconProcessor again.
            Configuration.Default.MemoryAllocator.ReleaseRetainedResources();
        }*/
    }
}
