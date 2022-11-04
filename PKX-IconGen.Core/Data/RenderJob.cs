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
        public RenderScale Scale => Settings.RenderScale;

        [JsonPropertyName("game")] 
        public Game Game => Settings.CurrentGame;

        [JsonIgnore]
        private Settings Settings { get; init; }

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

        public RenderJob(PokemonRenderData data, Settings settings)
        {
            Data = data;
            Settings = settings;
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

            stepOutput?.Invoke($"Rendering {Data.Name}...".AsMemory());
            CoreManager.Logger.Information("Rendering {Output} ({Name})...", Data.Output, Data.Name);
            await runner.RunAsync(token);
            CoreManager.Logger.Information("Rendering {Output} ({Name})...Done!", Data.Output, Data.Name);

            IconProcessor iconProcessor = new(this, Settings.OutputPath, Settings.SaturationBoost);
            await iconProcessor.ProcessJobAsync(token, stepOutput);
        }
    }
}
