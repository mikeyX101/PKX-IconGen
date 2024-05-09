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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PKXIconGen.Core.Data.Compatibility;
using PKXIconGen.Core.Services;

namespace PKXIconGen.Core.Data
{
    [Table("PokemonRenderData"), Microsoft.EntityFrameworkCore.Index(nameof(Id), IsUnique = true, Name = "IDX_ID"), JsonConverter(typeof(PokemonRenderDataJsonConverter))]
    public class PokemonRenderData : IJsonSerializable, IEquatable<PokemonRenderData>, ICloneable, INotifyPropertyChanged
    {
        [Column("ID"), Key, Required, JsonIgnore]
        public uint Id { get; internal set; }

        private string name;
        [Column, JsonPropertyName("name"), JsonRequired]
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string? outputName;
        [Column, JsonPropertyName("output_name")]
        public string? OutputName
        {
            get => outputName;
            set
            {
                outputName = value;
                OnPropertyChanged();
            }
        }
        
        [JsonIgnore]
        public string Output => string.IsNullOrWhiteSpace(OutputName) ? Name : OutputName;
        [JsonIgnore]
        public string FaceOutput => Output + "_face";
        [JsonIgnore]
        public string BodyOutput => Output + "_box_body";
        [JsonIgnore]
        public string BodyShinyOutput => Output + "_box_body_shiny";
        [JsonIgnore]
        public string DanceOutput => Output + "_box_dance";
        [JsonIgnore]
        public string DanceShinyOutput => Output + "_box_dance_shiny";

        private string model;
        /// <summary>
        /// Model path. Can contain {{AssetsPath}} to represent the path to extracted assets.
        /// </summary>
        [Column, JsonPropertyName("model"), JsonRequired]
        [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract", Justification = "False during init")]
        public string Model
        {
            get => Utils.CleanModelPathString(model);
            set
            {
                // Due to limitations, we need to empty the texture list and the removed objects list if the model is changed in case the model is actually different
                if (FaceRender?.Textures != null && FaceRender.Textures.Count != 0)
                {
                    FaceRender.Textures.Clear();
                    CoreManager.Logger.Information("Model changed while having textures set up, removing to avoid conflicts");
                }
                if (FaceRender?.RemovedObjects is not null && FaceRender.RemovedObjects.Count != 0)
                {
                    FaceRender.RemovedObjects.Clear();
                    CoreManager.Logger.Information("Model changed while having removed objects, resetting to avoid conflicts");
                }
                
                BoxRender?.ResetTexturesAndRemovedObjects();
                CachedNames = null;
                NamesCached = false;
                model = Utils.CleanModelPathString(value);
            }
        }
        
        [JsonPropertyName("render"), UsedImplicitly, Obsolete("Used for old JSON compatibility. Use FaceRender instead.")]
        public RenderData Render
        {
            set => faceRender = value;
        }
        private RenderData faceRender;
        [Column("FaceRender"), JsonPropertyName("face")]
        public RenderData FaceRender
        {
            get => faceRender;
            set
            {
                faceRender = value;
                OnPropertyChanged();
            }
        }

        private BoxInfo boxRender;
        [Column("BoxRender"), JsonPropertyName("box")]
        public BoxInfo BoxRender
        {
            get => boxRender;
            set
            {
                boxRender = value;
                OnPropertyChanged();
            }
        }
        
        private ShinyInfo shiny;
        [Column("Shiny"), JsonPropertyName("shiny")]
        public ShinyInfo Shiny 
        {
            get => shiny;
            set
            {
                shiny = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public string TemplateName
        {
            get
            {
                string shinyModel = !string.IsNullOrWhiteSpace(shiny.Model) ? $"_{Path.GetFileNameWithoutExtension(shiny.Model)}" : "";
                return $"{Id}_{Output}_{Path.GetFileNameWithoutExtension(Model)}{shinyModel}";
            }
        }

        public PokemonRenderData()
        {
            Id = 0;
            
            name = "";
            OutputName = "";

            model = "";

            faceRender = new RenderData();
            boxRender = new BoxInfo();
            shiny = new ShinyInfo();
        }
        
        public PokemonRenderData(uint id, string name, string? outputName, string model, RenderData faceRender, BoxInfo? boxRender, ShinyInfo shiny)
        {
            Id = id;

            this.name = name;
            OutputName = !string.IsNullOrEmpty(outputName) ? outputName : null;

            this.model = model;
            
            this.faceRender = faceRender;
            this.boxRender = boxRender ?? new BoxInfo();
            this.shiny = shiny;
        }

        [JsonConstructor]
        public PokemonRenderData(string name, string? outputName, string model, RenderData render, BoxInfo? boxRender, ShinyInfo shiny) 
            : this(0, name, outputName, model, render, boxRender, shiny)
        {

        }

        public async Task ModifyAsync(IBlenderRunnerInfo blenderRunnerInfo, CancellationToken token, IBlenderRunner.OutDel? onOutput = null, Action? onFinish = null)
        {
            IBlenderRunner runner = BlenderRunner.BlenderRunners.GetModifyDataRunner(blenderRunnerInfo, this);
            if (onOutput != null)
            {
                runner.OnOutput += onOutput;
            }
            
            runner.OnFinish += newData =>
            {
                if (newData is null) return;
                
                FaceRender = newData.FaceRender;
                BoxRender = newData.BoxRender;
                Shiny = newData.Shiny;
            };
            await runner.RunAsync(token);
            onFinish?.Invoke();
        }

        [JsonIgnore]
        private bool NamesCached { get; set; } = false;
        [JsonIgnore]
        private TextureNames? CachedNames { get; set; }
        public string? GetTextureNames(Game forGame, TextureTargetChoice texture, OutputChoice output)
        {
            string modelName = Path.GetFileNameWithoutExtension(Model);
            if (string.IsNullOrWhiteSpace(modelName))
            {
                return null;
            }

            if (!NamesCached)
            {
                NameMap.LoadNamesMap(forGame);

                CachedNames = NameMap.GetTextureNames(modelName);
                NamesCached = true;
            }

            return CachedNames?.GetName(texture, output);
        }

        public bool Equals(PokemonRenderData? other)
        {
            return other is not null &&
                Id == other.Id &&
                Name == other.Name &&
                Model == other.Model &&
                FaceRender.Equals(other.FaceRender) &&
                Shiny.Equals(other.Shiny);
        }
        public override bool Equals(object? obj)
        {
            return obj is PokemonRenderData prd && Equals(prd);
        }

        public static bool operator ==(PokemonRenderData? left, PokemonRenderData? right)
        {
            return left?.Equals(right) ?? left is null && right is null;
        }

        public static bool operator !=(PokemonRenderData? left, PokemonRenderData? right)
        {
            return !(left == right);
        }

        public override int GetHashCode() => 
            (
                Id, 
                Name,
                OutputName,
                Model,
                FaceRender,
                Shiny
            ).GetHashCode();

        public object Clone()
        {
            return new PokemonRenderData(Id, Name, OutputName, Model, (RenderData)FaceRender.Clone(), (BoxInfo)BoxRender.Clone(), (ShinyInfo)Shiny.Clone());
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
