using System;
using System.Threading.Tasks;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.ImageProcessing.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PKXIconGen.Core.ImageProcessing
{
    internal class IconProcessor : IDisposable
    {
        private Game Game { get; set; }

        internal IconProcessor(Game game)
        {
            Game = game;
        }

        internal async Task Process(string mainStr, string shinyStr, string outputStr)
        {
            Task<Image> mainLoadTask = Image.LoadAsync(mainStr);
            Task<Image> shinyLoadTask = Image.LoadAsync(shinyStr);

            using Image main = await mainLoadTask;
            Task mainGameProcessTask = GameProcess(main);

            using Image shiny = await shinyLoadTask;
            Task shinyGameProcessTask = GameProcess(shiny);

            await Task.WhenAll(mainGameProcessTask, shinyGameProcessTask);
            main.Mutate(ctx => ctx.AddImageBottom(shiny));

            await main.SaveAsPngAsync(outputStr);
        }

        internal async Task Process(string mainStr, string shinyStr, string secondaryStr, string sShinyStr, string outputStr)
        {
            Task<Image> secondaryLoadTask = Image.LoadAsync(secondaryStr);
            Task<Image> sShinyLoadTask = Image.LoadAsync(sShinyStr);
            Task<Image> mainLoadTask = Image.LoadAsync(mainStr);
            Task<Image> shinyLoadTask = Image.LoadAsync(shinyStr);

            using Image secondary = await mainLoadTask;
            Task secondaryGameProcessTask = GameProcess(secondary);

            using Image sShiny = await shinyLoadTask;
            Task sShinyGameProcessTask = GameProcess(sShiny);

            using Image main = await mainLoadTask;
            Task mainGameProcessTask = GameProcess(main);

            using Image shiny = await shinyLoadTask;
            Task shinyGameProcessTask = GameProcess(shiny);

            await Task.WhenAll(secondaryGameProcessTask, sShinyGameProcessTask);
            secondary.Mutate(ctx => ctx.AddImageBottom(sShiny));

            await Task.WhenAll(mainGameProcessTask, shinyGameProcessTask);
            main.Mutate(ctx => ctx.AddImageBottom(shiny).AddImageRight(secondary));

            await main.SaveAsPngAsync(outputStr);
        }

        private Task GameProcess(Image img)
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
            });
        }

        public void Dispose()
        {
            // We don't need ImageSharp unless we use IconProcessor again.
            Configuration.Default.MemoryAllocator.ReleaseRetainedResources();
        }
    }
}
