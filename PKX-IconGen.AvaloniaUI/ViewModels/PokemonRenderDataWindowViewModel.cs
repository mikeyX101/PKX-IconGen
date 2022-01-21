#region License
/*  PKX-IconGen.AvaloniaUI - Avalonia user interface for PKX-IconGen.Core
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaColors = Avalonia.Media.Colors;
using PKXIconGen.AvaloniaUI.Services;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Data.Blender;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.IO;
using PKXIconGen.Core.Services;
using System.Threading;
using System.Numerics;
using PKXIconGen.Core;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class PokemonRenderDataWindowViewModel : WindowViewModelBase
    {
        #region General
        public string Header
        {
            get
            {
                return $"Pokemon - {(string.IsNullOrWhiteSpace(Name) ? "???" : Name)}";
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set {
                this.RaiseAndSetIfChanged(ref name, value);
                this.RaisePropertyChanged(nameof(Header));
            }
        }

        private string model;
        public string Model
        {
            get => model;
            set => this.RaiseAndSetIfChanged(ref model, value);
        }
        #endregion

        #region Shiny
        private AvaloniaColor? shinyColor;
        public AvaloniaColor ShinyColor
        {
            get => shinyColor ?? AvaloniaColors.White;
            set
            {
                value = AvaloniaColor.FromArgb(255, value.R, value.G, value.B);
                this.RaiseAndSetIfChanged(ref shinyColor, value);
            }
        }

        private string? shinyModel;
        public string ShinyModel
        {
            get => shinyModel ?? "";
            set => this.RaiseAndSetIfChanged(ref shinyModel, value);
        }
        #endregion

        #region Blender Data
        private ushort normalAnimPose;
        public ushort NormalAnimPose {
            get => normalAnimPose;
            set => this.RaiseAndSetIfChanged(ref normalAnimPose, value);
        }
        private ushort normalAnimFrame;
        public ushort NormalAnimFrame
        {
            get => normalAnimFrame;
            set => this.RaiseAndSetIfChanged(ref normalAnimFrame, value);
        }

        private ushort shinyAnimPose;
        public ushort ShinyAnimPose
        {
            get => shinyAnimPose;
            set => this.RaiseAndSetIfChanged(ref shinyAnimPose, value);
        }
        private ushort shinyAnimFrame;
        public ushort ShinyAnimFrame
        {
            get => shinyAnimFrame;
            set => this.RaiseAndSetIfChanged(ref shinyAnimFrame, value);
        }

        private Camera mainCamera;
        public Camera MainCamera
        {
            get => mainCamera;
            set => this.RaiseAndSetIfChanged(ref mainCamera, value);
        }
        public IList<Light> MainLights { get; }
        private Camera? secondaryCamera;
        public Camera? SecondaryCamera {
            get => secondaryCamera;
            set
            {
                this.RaiseAndSetIfChanged(ref secondaryCamera, value);
                //TODO This is discusting, must be a better way to do this. Although this is not going to happen too often at least.
                this.RaisePropertyChanged(nameof(SCPosX));
                this.RaisePropertyChanged(nameof(SCPosY));
                this.RaisePropertyChanged(nameof(SCPosZ));
                this.RaisePropertyChanged(nameof(SCRotX));
                this.RaisePropertyChanged(nameof(SCRotY));
                this.RaisePropertyChanged(nameof(SCRotZ));
            }
        }
        public float? SCPosX => SecondaryCamera?.Position.X;
        public float? SCPosY => SecondaryCamera?.Position.X;
        public float? SCPosZ => SecondaryCamera?.Position.X;
        public float? SCRotX => SecondaryCamera?.RotationEuler.X;
        public float? SCRotY => SecondaryCamera?.RotationEuler.X;
        public float? SCRotZ => SecondaryCamera?.RotationEuler.X;
        public IList<Light> SecondaryLights { get; }
        #endregion

        public bool currentlyModifying = false;
        public bool CurrentlyModifying
        {
            get => currentlyModifying;
            set => this.RaiseAndSetIfChanged(ref currentlyModifying, value);
        }

        private IBlenderRunnerInfo BlenderRunnerInfo { get; }
        private bool useFilter;
        public bool UseFilter
        {
            get => useFilter;
            set => this.RaiseAndSetIfChanged(ref useFilter, value);
        }

#pragma warning disable CS8618 // Nullable field
        private PokemonRenderDataWindowViewModel()
#pragma warning restore CS8618 // Nullable field
        {
            // Reactive
            IObservable<bool> canModifyOrSave = this.WhenAnyValue(
                vm => vm.Name, vm => vm.Model,
                (name, model) => !string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(model) && File.Exists(model)
            );

            ModifyBlenderDataCommand = ReactiveCommand.Create(ModifyBlenderData, canModifyOrSave);
            CancelCommand = ReactiveCommand.Create(Cancel);
            SaveCommand = ReactiveCommand.Create(Save, canModifyOrSave);
        }
        public PokemonRenderDataWindowViewModel(IBlenderRunnerInfo blenderRunnerInfo) : this()
        {
            useFilter = true;

            name = "";
            model = "";

            shinyColor = null;
            shinyModel = "";

            normalAnimPose = 0;
            normalAnimFrame = 0;

            shinyAnimPose = 0;
            shinyAnimFrame = 0;

            MainCamera = Camera.GetDefaultCamera();
            MainLights = new ObservableCollection<Light>();
            SecondaryCamera = null;
            SecondaryLights = new ObservableCollection<Light>();

            BlenderRunnerInfo = blenderRunnerInfo;
        }
        public PokemonRenderDataWindowViewModel(IBlenderRunnerInfo blenderRunnerInfo, PokemonRenderData renderData) : this()
        {
            useFilter = renderData.Shiny.Filter != null;

            name = renderData.Name;
            model = renderData.Model;

            shinyColor = AvaloniaColors.White; // Color.FromArgb(renderData.Shiny.Filter.);
            shinyModel = renderData.Shiny.AltModel;

            normalAnimPose = renderData.AnimationPose;
            normalAnimFrame = renderData.AnimationFrame;

            shinyAnimPose = renderData.Shiny.AnimationPose;
            shinyAnimFrame = renderData.Shiny.AnimationFrame;

            MainCamera = renderData.MainCamera;
            MainLights = new ObservableCollection<Light>(renderData.MainLights);
            SecondaryCamera = renderData.SecondaryCamera;
            SecondaryLights = new ObservableCollection<Light>(renderData.SecondaryLights);

            BlenderRunnerInfo = blenderRunnerInfo;
        }

        public async void BrowseModelPath()
        {
            string? path = await SelectModel();
            if (path != null)
            {
                Model = path;
            }
        }

        public async void BrowseShinyModelPath()
        {
            string? path = await SelectModel();
            if (path != null)
            {
                ShinyModel = path;
            }
        }

        private async Task<string?> SelectModel()
        {
            List<FileDialogFilter>? filters = null;
            if (IsWindows)
            {
                filters = new();
                List<string> extensions = new()
                {
                    "dat"
                };
                filters.Add(new FileDialogFilter { Extensions = extensions, Name = "GCN/WII Pokemon Model" });
            }

            return await FileDialogHelper.GetFile("Select a model...", filters);
        }

        #region Modify in Blender
        private CancellationTokenSource? modifyBlenderDataCancelTokenSource = null;
        public ReactiveCommand<Unit, Unit> ModifyBlenderDataCommand { get; }
        public async void ModifyBlenderData()
        {
            DisposeCancelToken();
            modifyBlenderDataCancelTokenSource = new();

            BlenderRunner blenderRunner = new(BlenderRunnerInfo, new string[]
            {
                "--enable-autoexec",
                $"--python ${BlenderPythonScripts.SceneGenerator}"
            });
            blenderRunner.OnFinish += EndModifyBlenderData;

            CurrentlyModifying = true;
            await blenderRunner.RunAsync(modifyBlenderDataCancelTokenSource.Token);
        }
        public void EndModifyBlenderData()
        {
            DisposeCancelToken();
            CurrentlyModifying = false;
        }
        private void DisposeCancelToken()
        {
            // Make sure tokens are canceled
            if (modifyBlenderDataCancelTokenSource != null)
            {
                modifyBlenderDataCancelTokenSource.Cancel();
                modifyBlenderDataCancelTokenSource.Dispose();
                modifyBlenderDataCancelTokenSource = null;
            }
        }
        #endregion

        #region Save
        public ReactiveCommand<Unit, object?> CancelCommand { get; }
        public static object? Cancel() => null;
        public ReactiveCommand<Unit, PokemonRenderData> SaveCommand { get; }
        public PokemonRenderData Save() => 
            new(
                Name, Model, NormalAnimPose, NormalAnimFrame,
                UseFilter ? new ShinyInfo(Color.FromRgbInt(ShinyColor.ToUint32()), ShinyAnimPose, ShinyAnimFrame) : new ShinyInfo(ShinyModel, ShinyAnimPose, ShinyAnimFrame),
                MainCamera, MainLights.ToArray(), SecondaryCamera, SecondaryLights.ToArray()
            );
        #endregion
    }
}
