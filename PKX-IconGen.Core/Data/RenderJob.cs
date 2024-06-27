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

namespace PKXIconGen.Core.Data;

public class RenderJob : IJsonSerializable
{
    [JsonPropertyName("data")]
    public PokemonRenderData Data { get; }

    [JsonPropertyName("scale")]
    public RenderScale Scale => Settings.RenderScale;

    [JsonPropertyName("game")] 
    public Game Game => Settings.CurrentGame;
        
    [JsonPropertyName("target")] 
    public RenderTarget Target { get; }

    [JsonIgnore]
    private Settings Settings { get; }

    [JsonIgnore]
    private string BlenderOutputPath => Path.Combine(Paths.TempFolder, Data.Output);

    [JsonPropertyName("face_main_path")]
    public string FaceMainPath => BlenderOutputPath + "_face_main.png";
    [JsonPropertyName("face_shiny_path")]
    public string FaceShinyPath => BlenderOutputPath + "_face_shiny.png";
    [JsonPropertyName("face_secondary_path")]
    public string FaceSecondaryPath => BlenderOutputPath + "_face_secondary.png";
    [JsonPropertyName("face_shiny_secondary_path")]
    public string FaceShinySecondaryPath => BlenderOutputPath + "_face_shiny_secondary.png";
        
    [JsonPropertyName("box_first_main_path")]
    public string BoxFirstMainPath => BlenderOutputPath + "_box_first_main.png";
    [JsonPropertyName("box_first_shiny_path")]
    public string BoxFirstShinyPath => BlenderOutputPath + "_box_first_shiny.png";
    [JsonPropertyName("box_second_main_path")]
    public string BoxSecondMainPath => BlenderOutputPath + "_box_second_main.png";
    [JsonPropertyName("box_second_shiny_path")]
    public string BoxSecondShinyPath => BlenderOutputPath + "_box_second_shiny.png";
    [JsonPropertyName("box_third_main_path")]
    public string BoxThirdMainPath => BlenderOutputPath + "_box_third_main.png";
    [JsonPropertyName("box_third_shiny_path")]
    public string BoxThirdShinyPath => BlenderOutputPath + "_box_third_shiny.png";
        
    public RenderJob(PokemonRenderData data, Settings settings, RenderTarget target)
    {
        Data = data;
        Settings = settings;
        Target = target;
    }
        
    public async Task RenderAsync(CancellationToken? token = null, IBlenderRunner.OutDel? onOutput = null, IBlenderRunner.FinishDel? onFinish = null, Func<ReadOnlyMemory<char>, Task>? stepOutputAsync = null)
    {
        IBlenderRunner runner = BlenderRunner.BlenderRunners.GetRenderRunner(Settings, this);
        if (onOutput != null)
        {
            runner.OnOutput += onOutput;
        }
        if (onFinish != null)
        {
            runner.OnFinish += onFinish;
        }

        stepOutputAsync?.Invoke($"Rendering {Data.Name}...".AsMemory());
        CoreManager.Logger.Information("Rendering {Output} ({Name})...", Data.Output, Data.Name);
        await Task.Run(async() => await runner.RunAsync(token));
        CoreManager.Logger.Information("Rendering {Output} ({Name})...Done!", Data.Output, Data.Name);

        IconProcessor iconProcessor = new(this, Settings.OutputPath, Settings.SaturationBoost, Settings.SaveDanceGIF, Settings.OutputNameForGame, Settings.OutputNameForTarget);
        await Task.Run(async() => await iconProcessor.ProcessJobAsync(token, stepOutputAsync));
    }
}