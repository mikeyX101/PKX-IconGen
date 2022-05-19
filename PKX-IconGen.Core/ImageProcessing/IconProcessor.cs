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
