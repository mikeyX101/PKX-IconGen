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
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using PKXIconGen.Core.Services;

namespace PKXIconGen.Core.Data
{
    [Table("PokemonRenderData"), Microsoft.EntityFrameworkCore.Index(nameof(Id), IsUnique = true, Name = "IDX_ID")]
    public class PokemonRenderData : IJsonSerializable, IEquatable<PokemonRenderData>, ICloneable, INotifyPropertyChanged
    {
        [Column("ID"), Key, Required, JsonIgnore]
        public uint Id { get; internal set; }

        private string name;
        [Column, JsonPropertyName("name")]
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
        public string Output => !string.IsNullOrWhiteSpace(OutputName) ? OutputName : Name;

        private bool builtIn;
        [Column, JsonIgnore] 
        public bool BuiltIn
        {
            get => builtIn;
            set
            {
                builtIn = value;
                OnPropertyChanged();
            }
        }

        private RenderData render;
        [Column, JsonPropertyName("render")]
        public RenderData Render
        {
            get => render;
            set
            {
                render = value;
                OnPropertyChanged();
            }
        }
        private ShinyInfo shiny;
        [Column, JsonPropertyName("shiny")]
        public ShinyInfo Shiny 
        {
            get => shiny;
            set
            {
                shiny = value;
                OnPropertyChanged();
            }
        }

        public PokemonRenderData()
        {
            name = "";
            OutputName = "";
            builtIn = false;

            render = new RenderData();
            shiny = new ShinyInfo();
        }
        
        public PokemonRenderData(uint id, string name, string? outputName, bool builtIn, RenderData render, ShinyInfo shiny)
        {
            Id = id;

            this.name = name;
            OutputName = !string.IsNullOrEmpty(outputName) ? outputName : null;
            this.builtIn = builtIn;

            this.render = render;
            this.shiny = shiny;
        }

        [JsonConstructor]
        public PokemonRenderData(string name, string? outputName, RenderData render, ShinyInfo shiny) 
            : this(0, name, outputName, false, render, shiny)
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
                if (newData is not null)
                {
                    Render = newData.Render;
                    Shiny = newData.Shiny;
                }
            };
            await runner.RunAsync(token);
            onFinish?.Invoke();
        }

        public bool Equals(PokemonRenderData? other)
        {
            return other is not null &&
                Id == other.Id &&
                Name == other.Name &&
                BuiltIn == other.BuiltIn &&
                Render.Equals(other.Render) &&
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
                BuiltIn,
                Render,
                Shiny
            ).GetHashCode();

        public object Clone()
        {
            return new PokemonRenderData(Id, Name, OutputName, BuiltIn, (RenderData)Render.Clone(), (ShinyInfo)Shiny.Clone());
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
