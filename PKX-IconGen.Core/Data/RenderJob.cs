using PKXIconGen.Core.ImageProcessing;
using PKXIconGen.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

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
        internal string FinalOutput { get; init; }

        [JsonIgnore]
        internal string BlenderOutputPath => Path.Combine(Paths.TempFolder, Data.Output);

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
        
        public async Task RenderAsync(IBlenderRunnerInfo blenderRunnerInfo, CancellationToken token, Action<ReadOnlyMemory<char>>? onOutput = null, Action<PokemonRenderData?>? onFinish = null)
        {
            IBlenderRunner runner = BlenderRunner.BlenderRunners.GetRenderRunner(blenderRunnerInfo, this);
            if (onOutput != null)
            {
                runner.OnOutput += new IBlenderRunner.OutDel(onOutput);
            }
            if (onFinish != null)
            {
                runner.OnFinish += new IBlenderRunner.FinishDel(onFinish);
            }
            
            await runner.RunAsync(token);

            IconProcessor iconProcessor = new(this, FinalOutput);
            await iconProcessor.ProcessJobAsync();
        }
    }
}
