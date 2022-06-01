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
using System.IO;
using System.Threading.Tasks;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.ImageProcessing.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PKXIconGen.Core.ImageProcessing
{
    public class IconProcessor
    {
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

        public async Task ProcessJobAsync()
        {
            Task<Image> mainTask = ProcessIconAsync(Job.MainPath);
            Task<Image> shinyTask = ProcessIconAsync(Job.ShinyPath);
            Task<Image>? secondaryTask = HasSecondary ? ProcessIconAsync(Job.SecondaryPath) : null;
            Task<Image>? shinySecondaryTask = HasSecondary ? ProcessIconAsync(Job.ShinySecondaryPath) : null;

            Image[] mainImages = await Task.WhenAll(mainTask, shinyTask);
            Image main = mainImages[0];
            Image shiny = mainImages[1];
            main.Mutate(ctx => ctx.AddImageBottom(shiny));

            if (secondaryTask != null && shinySecondaryTask != null)
            {
                Image[] secondaryImages = await Task.WhenAll(secondaryTask, shinySecondaryTask);
                Image secondary = secondaryImages[0];
                Image shinySecondary = secondaryImages[1];
                secondary.Mutate(ctx => ctx.AddImageBottom(shinySecondary));

                main.Mutate(ctx => ctx.AddImageRight(secondary));
            }

            await main.SaveAsPngAsync(Path.Combine(FinalOutput, Job.Data.Output + ".png"));
        }

        private async Task<Image> ProcessIconAsync(string path)
        {
            Image img = await Image.LoadAsync(path);
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
