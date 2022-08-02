using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using PKXIconGen.Core.ImageProcessing;
using PKXIconGen.Core.Services;

namespace PKXIconGen.Core.Data
{
    public class RenderJob : IJsonSerializable
    {
        [JsonPropertyName("data")]
        public PokemonRenderData Data { get; init; }

        [JsonPropertyName("scale")]
        public RenderScale Scale { get; init; }

        [JsonPropertyName("game")]
        public Game Game { get; init; }

        [JsonIgnore]
        private string FinalOutput { get; init; }

        [JsonIgnore]
        private string BlenderOutputPath => Path.Combine(Paths.TempFolder, Data.Output);

        [JsonPropertyName("main_path")]
        public string MainPath => BlenderOutputPath + "_main.png";
        [JsonPropertyName("shiny_path")]
        public string ShinyPath => BlenderOutputPath + "_shiny.png";
        [JsonPropertyName("secondary_path")]
        public string SecondaryPath => BlenderOutputPath + "_secondary.png";
        [JsonPropertyName("shiny_secondary_path")]
        public string ShinySecondaryPath => BlenderOutputPath + "_shiny_secondary.png";

        public RenderJob(PokemonRenderData data, RenderScale scale, Game game, string finalOutput)
        {
            Data = data;
            Scale = scale;
            Game = game;
            FinalOutput = finalOutput;
        }
        
        public async Task RenderAsync(IBlenderRunnerInfo blenderRunnerInfo, CancellationToken? token = null, IBlenderRunner.OutDel? onOutput = null, IBlenderRunner.FinishDel? onFinish = null, Action<ReadOnlyMemory<char>>? stepOutput = null)
        {
            IBlenderRunner runner = BlenderRunner.BlenderRunners.GetRenderRunner(blenderRunnerInfo, this);
            if (onOutput != null)
            {
                runner.OnOutput += onOutput;
            }
            if (onFinish != null)
            {
                runner.OnFinish += onFinish;
            }

            stepOutput?.Invoke($"Rendering {Data.Output}...".AsMemory());
            CoreManager.Logger.Information("Rendering {Name}...", Data.Output);
            await runner.RunAsync(token);
            CoreManager.Logger.Information("Rendering {Name}...Done!", Data.Output);

            IconProcessor iconProcessor = new(this, FinalOutput);
            await iconProcessor.ProcessJobAsync(token, stepOutput);
        }
    }
}
